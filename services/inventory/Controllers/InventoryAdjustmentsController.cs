using DndApp.Inventory.Contracts;
using DndApp.Inventory.Data;
using DndApp.Inventory.Data.Entities;
using DndShop.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Inventory.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/inventory/adjustments")]
public sealed class InventoryAdjustmentsController(InventoryDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid campaignId,
        [FromQuery] int? fromWorldDay,
        [FromQuery] int? toWorldDay,
        [FromQuery] Guid? itemId,
        [FromQuery] Guid? storageLocationId,
        CancellationToken cancellationToken)
    {
        if (fromWorldDay.HasValue && fromWorldDay.Value < 0)
        {
            return BadRequest(new ErrorResponse("fromWorldDay must be greater than or equal to 0."));
        }

        if (toWorldDay.HasValue && toWorldDay.Value < 0)
        {
            return BadRequest(new ErrorResponse("toWorldDay must be greater than or equal to 0."));
        }

        if (fromWorldDay.HasValue && toWorldDay.HasValue && fromWorldDay.Value > toWorldDay.Value)
        {
            return BadRequest(new ErrorResponse("fromWorldDay must be less than or equal to toWorldDay."));
        }

        if (itemId.HasValue && itemId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("itemId must be a non-empty GUID when provided."));
        }

        if (storageLocationId.HasValue && storageLocationId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("storageLocationId must be a non-empty GUID when provided."));
        }

        var query = dbContext.InventoryAdjustments
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId);

        if (fromWorldDay.HasValue)
        {
            query = query.Where(x => x.WorldDay >= fromWorldDay.Value);
        }

        if (toWorldDay.HasValue)
        {
            query = query.Where(x => x.WorldDay <= toWorldDay.Value);
        }

        if (itemId.HasValue)
        {
            query = query.Where(x => x.ItemId == itemId.Value);
        }

        if (storageLocationId.HasValue)
        {
            query = query.Where(x => x.StorageLocationId == storageLocationId.Value);
        }

        var adjustments = await query
            .OrderBy(x => x.WorldDay)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new InventoryAdjustmentDto(
                x.AdjustmentId,
                x.ItemId,
                x.StorageLocationId,
                x.LotId,
                x.DeltaQuantity,
                x.Reason,
                x.WorldDay,
                x.Notes,
                x.ReferenceType,
                x.ReferenceId,
                x.CreatedByUserId,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(adjustments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateInventoryAdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetRequestingUserId(out var createdByUserId))
        {
            return Unauthorized();
        }

        var validationError = ValidateCreateRequest(request, out var normalizedRequest);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var locationExists = await dbContext.StorageLocations
            .AsNoTracking()
            .AnyAsync(
                x => x.CampaignId == campaignId && x.StorageLocationId == normalizedRequest.StorageLocationId,
                cancellationToken);

        if (!locationExists)
        {
            return BadRequest(new ErrorResponse("storageLocationId is invalid for this campaign."));
        }

        var now = DateTimeOffset.UtcNow;
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (normalizedRequest.LotId.HasValue)
        {
            var lot = await dbContext.InventoryLots
                .SingleOrDefaultAsync(
                    x => x.CampaignId == campaignId
                         && x.LotId == normalizedRequest.LotId.Value
                         && x.ItemId == normalizedRequest.ItemId
                         && x.StorageLocationId == normalizedRequest.StorageLocationId,
                    cancellationToken);

            if (lot is null)
            {
                return BadRequest(new ErrorResponse("lotId is invalid for itemId and storageLocationId in this campaign."));
            }

            var updatedQuantity = NormalizeQuantity(lot.QuantityOnHand + normalizedRequest.DeltaQuantity);
            if (updatedQuantity < 0)
            {
                return BadRequest(new ErrorResponse("Insufficient stock for the requested adjustment."));
            }

            lot.QuantityOnHand = updatedQuantity;
            lot.UpdatedAt = now;
        }
        else
        {
            var allocationError = await ApplyDeltaWithoutExplicitLotAsync(
                campaignId,
                normalizedRequest.ItemId,
                normalizedRequest.StorageLocationId,
                normalizedRequest.DeltaQuantity,
                now,
                cancellationToken);

            if (allocationError is not null)
            {
                return BadRequest(new ErrorResponse(allocationError));
            }
        }

        var adjustment = new InventoryAdjustment
        {
            AdjustmentId = Guid.NewGuid(),
            CampaignId = campaignId,
            ItemId = normalizedRequest.ItemId,
            StorageLocationId = normalizedRequest.StorageLocationId,
            LotId = normalizedRequest.LotId,
            DeltaQuantity = normalizedRequest.DeltaQuantity,
            Reason = normalizedRequest.Reason,
            WorldDay = normalizedRequest.WorldDay,
            Notes = normalizedRequest.Notes,
            ReferenceType = normalizedRequest.ReferenceType,
            ReferenceId = normalizedRequest.ReferenceId,
            CreatedByUserId = createdByUserId,
            CreatedAt = now
        };

        dbContext.InventoryAdjustments.Add(adjustment);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new CreateInventoryAdjustmentResponse(adjustment.AdjustmentId));
    }

    private async Task<string?> ApplyDeltaWithoutExplicitLotAsync(
        Guid campaignId,
        Guid itemId,
        Guid storageLocationId,
        decimal deltaQuantity,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var lots = await dbContext.InventoryLots
            .Where(x => x.CampaignId == campaignId
                        && x.ItemId == itemId
                        && x.StorageLocationId == storageLocationId)
            .OrderBy(x => x.AcquiredWorldDay)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        if (deltaQuantity > 0)
        {
            if (lots.Count == 0)
            {
                return "No lots found for this item and storage location. Provide lotId or create a lot first.";
            }

            var lot = lots[0];
            lot.QuantityOnHand = NormalizeQuantity(lot.QuantityOnHand + deltaQuantity);
            lot.UpdatedAt = now;
            return null;
        }

        var requiredQuantity = -deltaQuantity;
        var availableQuantity = lots.Sum(x => x.QuantityOnHand);

        if (availableQuantity < requiredQuantity)
        {
            return "Insufficient stock for the requested adjustment.";
        }

        var remainingToConsume = requiredQuantity;
        foreach (var lot in lots)
        {
            if (remainingToConsume <= 0)
            {
                break;
            }

            if (lot.QuantityOnHand <= 0)
            {
                continue;
            }

            var consumedQuantity = decimal.Min(lot.QuantityOnHand, remainingToConsume);
            lot.QuantityOnHand = NormalizeQuantity(lot.QuantityOnHand - consumedQuantity);
            lot.UpdatedAt = now;
            remainingToConsume = NormalizeQuantity(remainingToConsume - consumedQuantity);
        }

        return null;
    }

    private static string? ValidateCreateRequest(
        CreateInventoryAdjustmentRequest request,
        out NormalizedCreateInventoryAdjustmentRequest normalizedRequest)
    {
        if (request.ItemId == Guid.Empty)
        {
            normalizedRequest = default;
            return "itemId is required.";
        }

        if (request.StorageLocationId == Guid.Empty)
        {
            normalizedRequest = default;
            return "storageLocationId is required.";
        }

        if (request.LotId.HasValue && request.LotId.Value == Guid.Empty)
        {
            normalizedRequest = default;
            return "lotId must be a non-empty GUID when provided.";
        }

        var deltaQuantity = NormalizeQuantity(request.DeltaQuantity);
        if (deltaQuantity == 0)
        {
            normalizedRequest = default;
            return "deltaQuantity must be non-zero.";
        }

        if (!Enum.TryParse<AdjustmentReason>(request.Reason?.Trim(), ignoreCase: true, out var parsedReason))
        {
            normalizedRequest = default;
            return "reason is invalid.";
        }

        if (request.WorldDay < 0)
        {
            normalizedRequest = default;
            return "worldDay must be greater than or equal to 0.";
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 500)
        {
            normalizedRequest = default;
            return "notes must be 500 characters or fewer.";
        }

        var referenceType = string.IsNullOrWhiteSpace(request.ReferenceType) ? null : request.ReferenceType.Trim();
        if (referenceType?.Length > 50)
        {
            normalizedRequest = default;
            return "referenceType must be 50 characters or fewer.";
        }

        if (request.ReferenceId.HasValue && request.ReferenceId.Value == Guid.Empty)
        {
            normalizedRequest = default;
            return "referenceId must be a non-empty GUID when provided.";
        }

        normalizedRequest = new NormalizedCreateInventoryAdjustmentRequest(
            request.ItemId,
            request.StorageLocationId,
            request.LotId,
            deltaQuantity,
            parsedReason.ToString(),
            request.WorldDay,
            notes,
            referenceType,
            request.ReferenceId);

        return null;
    }

    private readonly record struct NormalizedCreateInventoryAdjustmentRequest(
        Guid ItemId,
        Guid StorageLocationId,
        Guid? LotId,
        decimal DeltaQuantity,
        string Reason,
        int WorldDay,
        string? Notes,
        string? ReferenceType,
        Guid? ReferenceId);
}
