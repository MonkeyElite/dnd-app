namespace DndApp.Catalog.Data.Entities;

public sealed class Item
{
    public Guid ItemId { get; set; }

    public Guid CampaignId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid CategoryId { get; set; }

    public Guid UnitId { get; set; }

    public long BaseValueMinor { get; set; }

    public long? DefaultListPriceMinor { get; set; }

    public decimal? Weight { get; set; }

    public Guid? ImageAssetId { get; set; }

    public bool IsArchived { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public Category? Category { get; set; }

    public Unit? Unit { get; set; }

    public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
}
