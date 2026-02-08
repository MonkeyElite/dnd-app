namespace DndApp.Catalog.Data.Entities;

public sealed class Tag
{
    public Guid TagId { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
}
