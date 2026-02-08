using DndApp.Campaign.Contracts;
using DndApp.Campaign.Data;
using DndApp.Campaign.Data.Entities;
using DndApp.Campaign.Defaults;
using DndApp.Campaign.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Campaign.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns")]
public sealed class CampaignsController(CampaignDbContext dbContext) : AuthenticatedControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateCampaignRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        var campaignId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var months = DefaultCampaignSettings.CreateMonths();
        var denominations = DefaultCampaignSettings.CreateDenominations();

        dbContext.Campaigns.Add(new Data.Entities.Campaign
        {
            CampaignId = campaignId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            CreatedByUserId = requestingUserId,
            CreatedAt = now
        });

        dbContext.CalendarConfigs.Add(new CalendarConfig
        {
            CampaignId = campaignId,
            WeekLength = DefaultCampaignSettings.WeekLength,
            MonthsJson = JsonArraySerializer.Serialize(months)
        });

        dbContext.CurrencyConfigs.Add(new CurrencyConfig
        {
            CampaignId = campaignId,
            CurrencyCode = DefaultCampaignSettings.CurrencyCode,
            MinorUnitName = DefaultCampaignSettings.MinorUnitName,
            MajorUnitName = DefaultCampaignSettings.MajorUnitName,
            DenominationsJson = JsonArraySerializer.Serialize(denominations)
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtRoute(nameof(GetByIdAsync), new { campaignId }, new CreateCampaignResponse(campaignId));
    }

    [HttpGet("{campaignId:guid}", Name = nameof(GetByIdAsync))]
    public async Task<IActionResult> GetByIdAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var campaign = await dbContext.Campaigns
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId)
            .Select(x => new CampaignResponse(
                x.CampaignId,
                x.Name,
                x.Description,
                x.CreatedByUserId,
                x.CreatedAt))
            .SingleOrDefaultAsync(cancellationToken);

        if (campaign is null)
        {
            return NotFound(new ErrorResponse("Campaign not found."));
        }

        return Ok(campaign);
    }

    private static string? ValidateCreateCampaignRequest(CreateCampaignRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "name is required.";
        }

        if (request.Name.Trim().Length > 100)
        {
            return "name must be 100 characters or fewer.";
        }

        if (request.Description?.Trim().Length > 500)
        {
            return "description must be 500 characters or fewer.";
        }

        return null;
    }
}
