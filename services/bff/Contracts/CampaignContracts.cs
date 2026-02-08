namespace DndApp.Bff.Contracts;

public sealed record CampaignSummaryDto(Guid CampaignId, string Name, string Role);

public sealed record CampaignsPageResponse(IReadOnlyList<CampaignSummaryDto> Campaigns);

public sealed record CreateCampaignRequest(string Name, string? Description);

public sealed record CreateCampaignResponse(Guid CampaignId);

public sealed record CalendarMonthDto(string Key, string Name, int Days);

public sealed record CalendarConfigDto(Guid CampaignId, int WeekLength, IReadOnlyList<CalendarMonthDto> Months);

public sealed record CurrencyDenominationDto(string Name, int Multiplier);

public sealed record CurrencyConfigDto(
    Guid CampaignId,
    string CurrencyCode,
    string MinorUnitName,
    string MajorUnitName,
    IReadOnlyList<CurrencyDenominationDto> Denominations);

public sealed record CampaignSettingsPageResponse(
    Guid CampaignId,
    string MyRole,
    CalendarConfigDto Calendar,
    CurrencyConfigDto Currency);

public sealed record UpdateCalendarSettingsActionRequest(Guid CampaignId, CalendarConfigDto Calendar);

public sealed record UpdateCurrencySettingsActionRequest(Guid CampaignId, CurrencyConfigDto Currency);

public sealed record UpdateCampaignSettingsResponse(bool Updated);

public sealed record CampaignDetailsDto(
    Guid CampaignId,
    string Name,
    string? Description,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt);

public sealed record CampaignCalendarUpdateRequest(int WeekLength, IReadOnlyList<CalendarMonthDto> Months);

public sealed record CampaignCurrencyUpdateRequest(
    string CurrencyCode,
    string MinorUnitName,
    string MajorUnitName,
    IReadOnlyList<CurrencyDenominationDto> Denominations);

public sealed record IdentityUpsertCampaignMembershipRequest(Guid CampaignId, Guid UserId, string Role);

public sealed record IdentityCampaignMembershipDto(Guid CampaignId, string Role);

public sealed record IdentityCampaignMemberMeDto(Guid CampaignId, Guid UserId, string Role);
