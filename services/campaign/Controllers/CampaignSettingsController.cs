using DndApp.Campaign.Contracts;
using DndApp.Campaign.Data;
using DndApp.Campaign.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Campaign.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/settings")]
public sealed class CampaignSettingsController(CampaignDbContext dbContext) : ControllerBase
{
    [HttpGet("calendar")]
    public async Task<IActionResult> GetCalendarAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var config = await dbContext.CalendarConfigs
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId, cancellationToken);

        if (config is null)
        {
            return NotFound(new ErrorResponse("Calendar settings not found."));
        }

        var months = JsonArraySerializer.Deserialize<CalendarMonthDto>(config.MonthsJson);
        return Ok(new CalendarConfigDto(campaignId, config.WeekLength, months));
    }

    [HttpPut("calendar")]
    public async Task<IActionResult> UpdateCalendarAsync(
        Guid campaignId,
        [FromBody] UpdateCalendarConfigRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCalendarRequest(request, out var normalizedMonths);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var config = await dbContext.CalendarConfigs
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId, cancellationToken);

        if (config is null)
        {
            return NotFound(new ErrorResponse("Calendar settings not found."));
        }

        config.WeekLength = request.WeekLength;
        config.MonthsJson = JsonArraySerializer.Serialize(normalizedMonths);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new UpdateResultResponse(true));
    }

    [HttpGet("currency")]
    public async Task<IActionResult> GetCurrencyAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var config = await dbContext.CurrencyConfigs
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId, cancellationToken);

        if (config is null)
        {
            return NotFound(new ErrorResponse("Currency settings not found."));
        }

        var denominations = JsonArraySerializer.Deserialize<CurrencyDenominationDto>(config.DenominationsJson);

        return Ok(new CurrencyConfigDto(
            campaignId,
            config.CurrencyCode,
            config.MinorUnitName,
            config.MajorUnitName,
            denominations));
    }

    [HttpPut("currency")]
    public async Task<IActionResult> UpdateCurrencyAsync(
        Guid campaignId,
        [FromBody] UpdateCurrencyConfigRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCurrencyRequest(request, out var normalizedDenominations);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var config = await dbContext.CurrencyConfigs
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId, cancellationToken);

        if (config is null)
        {
            return NotFound(new ErrorResponse("Currency settings not found."));
        }

        config.CurrencyCode = request.CurrencyCode.Trim();
        config.MinorUnitName = request.MinorUnitName.Trim();
        config.MajorUnitName = request.MajorUnitName.Trim();
        config.DenominationsJson = JsonArraySerializer.Serialize(normalizedDenominations);

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new UpdateResultResponse(true));
    }

    private static string? ValidateCalendarRequest(
        UpdateCalendarConfigRequest request,
        out IReadOnlyList<CalendarMonthDto> normalizedMonths)
    {
        if (request.WeekLength <= 0)
        {
            normalizedMonths = [];
            return "weekLength must be greater than 0.";
        }

        if (request.Months is null || request.Months.Count < 1)
        {
            normalizedMonths = [];
            return "months must contain at least one entry.";
        }

        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var mappedMonths = new List<CalendarMonthDto>(request.Months.Count);

        foreach (var month in request.Months)
        {
            var key = month.Key?.Trim() ?? string.Empty;
            var name = month.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(key))
            {
                normalizedMonths = [];
                return "month key is required.";
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                normalizedMonths = [];
                return "month name is required.";
            }

            if (month.Days <= 0)
            {
                normalizedMonths = [];
                return "month days must be greater than 0.";
            }

            if (!keys.Add(key))
            {
                normalizedMonths = [];
                return "month keys must be unique.";
            }

            mappedMonths.Add(new CalendarMonthDto(key, name, month.Days));
        }

        normalizedMonths = mappedMonths;
        return null;
    }

    private static string? ValidateCurrencyRequest(
        UpdateCurrencyConfigRequest request,
        out IReadOnlyList<CurrencyDenominationDto> normalizedDenominations)
    {
        if (string.IsNullOrWhiteSpace(request.CurrencyCode))
        {
            normalizedDenominations = [];
            return "currencyCode is required.";
        }

        if (string.IsNullOrWhiteSpace(request.MinorUnitName))
        {
            normalizedDenominations = [];
            return "minorUnitName is required.";
        }

        if (string.IsNullOrWhiteSpace(request.MajorUnitName))
        {
            normalizedDenominations = [];
            return "majorUnitName is required.";
        }

        if (request.Denominations is null)
        {
            normalizedDenominations = [];
            return "denominations is required.";
        }

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var mappedDenominations = new List<CurrencyDenominationDto>(request.Denominations.Count);

        foreach (var denomination in request.Denominations)
        {
            var name = denomination.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                normalizedDenominations = [];
                return "denomination name is required.";
            }

            if (denomination.Multiplier < 1)
            {
                normalizedDenominations = [];
                return "denomination multiplier must be at least 1.";
            }

            if (!names.Add(name))
            {
                normalizedDenominations = [];
                return "denomination names must be unique.";
            }

            mappedDenominations.Add(new CurrencyDenominationDto(name, denomination.Multiplier));
        }

        normalizedDenominations = mappedDenominations;
        return null;
    }
}
