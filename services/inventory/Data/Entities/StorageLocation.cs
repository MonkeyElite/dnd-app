namespace DndApp.Inventory.Data.Entities;

public sealed class StorageLocation
{
    public Guid StorageLocationId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid? PlaceId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Code { get; set; }

    public string Type { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<InventoryLot> Lots { get; set; } = new List<InventoryLot>();

    public ICollection<InventoryAdjustment> Adjustments { get; set; } = new List<InventoryAdjustment>();
}
