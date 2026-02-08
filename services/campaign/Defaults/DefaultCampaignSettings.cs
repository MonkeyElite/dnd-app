using DndApp.Campaign.Contracts;

namespace DndApp.Campaign.Defaults;

public static class DefaultCampaignSettings
{
    public const int WeekLength = 10;

    public const string CurrencyCode = "GSC";

    public const string MinorUnitName = "copper";

    public const string MajorUnitName = "gold";

    public static IReadOnlyList<CalendarMonthDto> CreateMonths()
        =>
        [
            new("BLOOM", "Season of Bloom", 70),
            new("HEAT", "Season of Heat", 70),
            new("STORMS", "Season of Storms", 70),
            new("MISTS", "Season of Mists", 70),
            new("FROST", "Season of Frost", 70)
        ];

    public static IReadOnlyList<CurrencyDenominationDto> CreateDenominations()
        =>
        [
            new("copper", 1),
            new("silver", 100),
            new("gold", 10000)
        ];
}
