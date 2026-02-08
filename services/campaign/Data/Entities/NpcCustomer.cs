namespace DndApp.Campaign.Data.Entities;

public sealed class NpcCustomer
{
    public Guid CustomerId { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string TagsJson { get; set; } = "[]";

    public Campaign? Campaign { get; set; }
}
