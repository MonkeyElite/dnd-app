namespace DndApp.Catalog.Data.Entities;

public sealed class Unit
{
    public Guid UnitId { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Item> Items { get; set; } = new List<Item>();
}
