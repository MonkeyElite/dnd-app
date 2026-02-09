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
[Route("campaigns/{campaignId:guid}/inventory/lots")]
public sealed class InventoryLotsController(InventoryDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid campaignId,
        [FromQuery] Guid? itemId,
        [FromQuery] Guid? storageLocationId,
        CancellationToken cancellationToken)
    {
        if (itemId.HasValue && itemId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("itemId must be a non-empty GUID when provided."));
        }

        if (storageLocationId.HasValue && storageLocationId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("storageLocationId must be a non-empty GUID when provided."));
        }

        var query = dbContext.InventoryLots
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId);

        if (itemId.HasValue)
        {
            query = query.Where(x => x.ItemId == itemId.Value);
        }

        if (storageLocationId.HasValue)
        {
            query = query.Where(x => x.StorageLocationId == storageLocationId.Value);
        }

        var lots = await query
            .OrderBy(x => x.AcquiredWorldDay)
            .ThenBy(x => x.CreatedAt)
            .Select(x => new InventoryLotDto(
                x.LotId,
                x.ItemId,
                x.StorageLocationId,
                x.QuantityOnHand,
                x.UnitCostMinor,
                x.AcquiredWorldDay,
                x.Source,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(lots);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateInventoryLotRequest request,
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
        var lot = new InventoryLot
        {
            LotId = Guid.NewGuid(),
            CampaignId = campaignId,
            ItemId = normalizedRequest.ItemId,
            StorageLocationId = normalizedRequest.StorageLocationId,
            QuantityOnHand = normalizedRequest.Quantity,
            UnitCostMinor = normalizedRequest.UnitCostMinor,
            AcquiredWorldDay = normalizedRequest.AcquiredWorldDay,
            Source = normalizedRequest.Source,
            Notes = normalizedRequest.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        var adjustment = new InventoryAdjustment
        {
            AdjustmentId = Guid.NewGuid(),
            CampaignId = campaignId,
            ItemId = normalizedRequest.ItemId,
            StorageLocationId = normalizedRequest.StorageLocationId,
            LotId = lot.LotId,
            DeltaQuantity = normalizedRequest.Quantity,
            Reason = AdjustmentReason.Restock.ToString(),
            WorldDay = normalizedRequest.AcquiredWorldDay,
            Notes = normalizedRequest.Notes,
            CreatedByUserId = createdByUserId,
            CreatedAt = now
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        dbContext.InventoryLots.Add(lot);
        dbContext.InventoryAdjustments.Add(adjustment);
        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Ok(new CreateInventoryLotResponse(lot.LotId));
    }

    private static string? ValidateCreateRequest(
        CreateInventoryLotRequest request,
        out NormalizedCreateInventoryLotRequest normalizedRequest)
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

        var quantity = NormalizeQuantity(request.Quantity);
        if (quantity <= 0)
        {
            normalizedRequest = default;
            return "quantity must be greater than 0.";
        }

        if (request.UnitCostMinor < 0)
        {
            normalizedRequest = default;
            return "unitCostMinor must be greater than or equal to 0.";
        }

        if (request.AcquiredWorldDay < 0)
        {
            normalizedRequest = default;
            return "acquiredWorldDay must be greater than or equal to 0.";
        }

        var source = string.IsNullOrWhiteSpace(request.Source) ? null : request.Source.Trim();
        if (source?.Length > 200)
        {
            normalizedRequest = default;
            return "source must be 200 characters or fewer.";
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 500)
        {
            normalizedRequest = default;
            return "notes must be 500 characters or fewer.";
        }

        normalizedRequest = new NormalizedCreateInventoryLotRequest(
            request.ItemId,
            request.StorageLocationId,
            quantity,
            request.UnitCostMinor,
            request.AcquiredWorldDay,
            source,
            notes);

        return null;
    }

    private readonly record struct NormalizedCreateInventoryLotRequest(
        Guid ItemId,
        Guid StorageLocationId,
        decimal Quantity,
        long UnitCostMinor,
        int AcquiredWorldDay,
        string? Source,
        string? Notes);
}
