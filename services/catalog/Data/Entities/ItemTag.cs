namespace DndApp.Catalog.Data.Entities;

public sealed class ItemTag
{
    public Guid ItemId { get; set; }

    public Guid TagId { get; set; }

    public Item? Item { get; set; }

    public Tag? Tag { get; set; }
}
