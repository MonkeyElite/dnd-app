namespace DndApp.Catalog.Data.Entities;

public sealed class Category
{
    public Guid CategoryId { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<Item> Items { get; set; } = new List<Item>();
}
