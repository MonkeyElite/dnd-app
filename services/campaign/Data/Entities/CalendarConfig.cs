namespace DndApp.Campaign.Data.Entities;

public sealed class CalendarConfig
{
    public Guid CampaignId { get; set; }

    public int WeekLength { get; set; }

    public string MonthsJson { get; set; } = "[]";

    public Campaign? Campaign { get; set; }
}
