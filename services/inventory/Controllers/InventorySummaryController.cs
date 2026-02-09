using DndApp.Inventory.Contracts;
using DndApp.Inventory.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Inventory.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/inventory/summary")]
public sealed class InventorySummaryController(InventoryDbContext dbContext) : InventoryControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid campaignId,
        [FromQuery] Guid? placeId,
        [FromQuery] Guid? storageLocationId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        if (placeId.HasValue && placeId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("placeId must be a non-empty GUID when provided."));
        }

        if (storageLocationId.HasValue && storageLocationId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("storageLocationId must be a non-empty GUID when provided."));
        }

        var locationQuery = dbContext.StorageLocations
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId);

        if (placeId.HasValue)
        {
            locationQuery = locationQuery.Where(x => x.PlaceId == placeId.Value);
        }

        if (storageLocationId.HasValue)
        {
            locationQuery = locationQuery.Where(x => x.StorageLocationId == storageLocationId.Value);
        }

        var summaryRows = await dbContext.InventoryLots
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId && x.QuantityOnHand > 0)
            .Join(
                locationQuery,
                lot => lot.StorageLocationId,
                location => location.StorageLocationId,
                (lot, location) => new
                {
                    lot.ItemId,
                    lot.StorageLocationId,
                    lot.QuantityOnHand
                })
            .GroupBy(x => new { x.ItemId, x.StorageLocationId })
            .Select(x => new InventorySummaryRowDto(
                x.Key.ItemId,
                x.Key.StorageLocationId,
                NormalizeQuantity(x.Sum(y => y.QuantityOnHand))))
            .OrderBy(x => x.ItemId)
            .ThenBy(x => x.StorageLocationId)
            .ToListAsync(cancellationToken);

        _ = search;
        return Ok(new InventorySummaryResponse(summaryRows));
    }
}
