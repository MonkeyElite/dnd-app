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
[Route("campaigns/{campaignId:guid}/storage-locations")]
public sealed class StorageLocationsController(InventoryDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid campaignId,
        [FromQuery] Guid? placeId,
        CancellationToken cancellationToken)
    {
        if (placeId.HasValue && placeId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("placeId must be a non-empty GUID when provided."));
        }

        var query = dbContext.StorageLocations
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId);

        if (placeId.HasValue)
        {
            query = query.Where(x => x.PlaceId == placeId.Value);
        }

        var locations = await query
            .OrderBy(x => x.Name)
            .Select(x => new StorageLocationDto(
                x.StorageLocationId,
                x.CampaignId,
                x.PlaceId,
                x.Name,
                x.Code,
                x.Type,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Ok(locations);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateStorageLocationRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateRequest(request, out var normalizedRequest);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var nameExists = await dbContext.StorageLocations
            .AsNoTracking()
            .AnyAsync(
                x => x.CampaignId == campaignId && x.Name == normalizedRequest.Name,
                cancellationToken);

        if (nameExists)
        {
            return Conflict(new ErrorResponse("Storage location name already exists for this campaign."));
        }

        var now = DateTimeOffset.UtcNow;
        var location = new StorageLocation
        {
            StorageLocationId = Guid.NewGuid(),
            CampaignId = campaignId,
            PlaceId = normalizedRequest.PlaceId,
            Name = normalizedRequest.Name,
            Code = normalizedRequest.Code,
            Type = normalizedRequest.Type,
            Notes = normalizedRequest.Notes,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.StorageLocations.Add(location);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict(new ErrorResponse("Storage location name already exists for this campaign."));
        }

        return Ok(new CreateStorageLocationResponse(location.StorageLocationId));
    }

    private static string? ValidateCreateRequest(
        CreateStorageLocationRequest request,
        out NormalizedCreateStorageLocationRequest normalizedRequest)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            normalizedRequest = default;
            return "name is required.";
        }

        if (name.Length > 100)
        {
            normalizedRequest = default;
            return "name must be 100 characters or fewer.";
        }

        var code = string.IsNullOrWhiteSpace(request.Code) ? null : request.Code.Trim();
        if (code?.Length > 50)
        {
            normalizedRequest = default;
            return "code must be 50 characters or fewer.";
        }

        if (!Enum.TryParse<StorageLocationType>(request.Type?.Trim(), ignoreCase: true, out var parsedType))
        {
            normalizedRequest = default;
            return "type is invalid.";
        }

        var notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        if (notes?.Length > 500)
        {
            normalizedRequest = default;
            return "notes must be 500 characters or fewer.";
        }

        if (request.PlaceId.HasValue && request.PlaceId.Value == Guid.Empty)
        {
            normalizedRequest = default;
            return "placeId must be a non-empty GUID when provided.";
        }

        normalizedRequest = new NormalizedCreateStorageLocationRequest(
            request.PlaceId,
            name,
            code,
            parsedType.ToString(),
            notes);

        return null;
    }

    private readonly record struct NormalizedCreateStorageLocationRequest(
        Guid? PlaceId,
        string Name,
        string? Code,
        string Type,
        string? Notes);
}
