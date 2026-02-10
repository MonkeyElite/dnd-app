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

public sealed record CampaignPlaceDto(Guid PlaceId, Guid CampaignId, string Name, string Type, string? Notes);

public sealed record CampaignCustomerDto(
    Guid CustomerId,
    Guid CampaignId,
    string Name,
    string? Notes,
    IReadOnlyList<string> Tags);

public sealed record CampaignCalendarUpdateRequest(int WeekLength, IReadOnlyList<CalendarMonthDto> Months);

public sealed record CampaignCurrencyUpdateRequest(
    string CurrencyCode,
    string MinorUnitName,
    string MajorUnitName,
    IReadOnlyList<CurrencyDenominationDto> Denominations);

public sealed record IdentityUpsertCampaignMembershipRequest(Guid CampaignId, Guid UserId, string Role);

public sealed record IdentityCampaignMembershipDto(Guid CampaignId, string Role);

public sealed record IdentityCampaignMemberMeDto(Guid CampaignId, Guid UserId, string Role);

public sealed record IdentityCampaignMemberDto(
    Guid CampaignId,
    Guid UserId,
    string Username,
    string DisplayName,
    string Role,
    bool IsPlatformAdmin);

public sealed record CampaignHomePageResponse(
    Guid CampaignId,
    string CampaignName,
    string? CampaignDescription,
    string MyRole,
    int CurrentWorldDay,
    CalendarConfigDto Calendar,
    CurrencyConfigDto Currency);

public sealed record CampaignSettingsMemberDto(
    Guid UserId,
    string Username,
    string DisplayName,
    string Role,
    bool IsPlatformAdmin);

public sealed record CampaignSettingsDetailsPageResponse(
    Guid CampaignId,
    string CampaignName,
    string? CampaignDescription,
    string MyRole,
    CalendarConfigDto Calendar,
    CurrencyConfigDto Currency,
    IReadOnlyList<CampaignSettingsMemberDto> Members);
