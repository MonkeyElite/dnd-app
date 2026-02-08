namespace DndApp.Campaign.Data.Entities;

public sealed class Place
{
    public Guid PlaceId { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public Campaign? Campaign { get; set; }
}
