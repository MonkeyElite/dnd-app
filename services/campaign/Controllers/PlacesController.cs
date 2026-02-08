using DndApp.Campaign.Contracts;
using DndApp.Campaign.Data;
using DndApp.Campaign.Data.Entities;
using DndShop.Contracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Campaign.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/places")]
public sealed class PlacesController(CampaignDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var places = await dbContext.Places
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId)
            .OrderBy(x => x.Name)
            .Select(x => new PlaceDto(x.PlaceId, x.CampaignId, x.Name, x.Type, x.Notes))
            .ToListAsync(cancellationToken);

        return Ok(places);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreatePlaceRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreatePlaceRequest(request, out var normalizedType);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var campaignExists = await dbContext.Campaigns
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId == campaignId, cancellationToken);

        if (!campaignExists)
        {
            return NotFound(new ErrorResponse("Campaign not found."));
        }

        var place = new Place
        {
            PlaceId = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = request.Name.Trim(),
            Type = normalizedType,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };

        dbContext.Places.Add(place);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreatePlaceResponse(place.PlaceId));
    }

    private static string? ValidateCreatePlaceRequest(CreatePlaceRequest request, out string normalizedType)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            normalizedType = string.Empty;
            return "name is required.";
        }

        if (request.Name.Trim().Length > 100)
        {
            normalizedType = string.Empty;
            return "name must be 100 characters or fewer.";
        }

        if (!Enum.TryParse<PlaceType>(request.Type?.Trim(), ignoreCase: true, out var parsedType))
        {
            normalizedType = string.Empty;
            return "type is invalid.";
        }

        if (request.Notes?.Trim().Length > 500)
        {
            normalizedType = string.Empty;
            return "notes must be 500 characters or fewer.";
        }

        normalizedType = parsedType.ToString();
        return null;
    }
}
