using DndApp.Catalog.Contracts;
using DndApp.Catalog.Data;
using DndApp.Catalog.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Catalog.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/units")]
public sealed class UnitsController(CatalogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var units = await dbContext.Units
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId)
            .OrderBy(x => x.Name)
            .Select(x => new UnitDto(x.UnitId, x.CampaignId, x.Name))
            .ToListAsync(cancellationToken);

        return Ok(units);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateUnitRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request, out var normalizedName);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var nameExists = await dbContext.Units
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId == campaignId && x.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            return Conflict(new ErrorResponse("Unit name already exists for this campaign."));
        }

        var unit = new Unit
        {
            UnitId = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = normalizedName
        };

        dbContext.Units.Add(unit);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateUnitResponse(unit.UnitId));
    }

    private static string? ValidateRequest(CreateUnitRequest request, out string normalizedName)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            normalizedName = string.Empty;
            return "name is required.";
        }

        if (name.Length > 50)
        {
            normalizedName = string.Empty;
            return "name must be 50 characters or fewer.";
        }

        normalizedName = name;
        return null;
    }
}
