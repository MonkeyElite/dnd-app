namespace DndApp.Campaign.Data.Entities;

public sealed class Campaign
{
    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public CalendarConfig? CalendarConfig { get; set; }

    public CurrencyConfig? CurrencyConfig { get; set; }

    public ICollection<Place> Places { get; set; } = new List<Place>();

    public ICollection<NpcCustomer> NpcCustomers { get; set; } = new List<NpcCustomer>();
}
