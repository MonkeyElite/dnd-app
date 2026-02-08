namespace DndApp.Campaign.Contracts;

public sealed record CalendarMonthDto(string Key, string Name, int Days);

public sealed record CalendarConfigDto(Guid CampaignId, int WeekLength, IReadOnlyList<CalendarMonthDto> Months);

public sealed record UpdateCalendarConfigRequest(int WeekLength, IReadOnlyList<CalendarMonthDto> Months);

public sealed record CurrencyDenominationDto(string Name, int Multiplier);

public sealed record CurrencyConfigDto(
    Guid CampaignId,
    string CurrencyCode,
    string MinorUnitName,
    string MajorUnitName,
    IReadOnlyList<CurrencyDenominationDto> Denominations);

public sealed record UpdateCurrencyConfigRequest(
    string CurrencyCode,
    string MinorUnitName,
    string MajorUnitName,
    IReadOnlyList<CurrencyDenominationDto> Denominations);
