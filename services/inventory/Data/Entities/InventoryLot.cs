namespace DndApp.Inventory.Data.Entities;

public sealed class InventoryLot
{
    public Guid LotId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid ItemId { get; set; }

    public Guid StorageLocationId { get; set; }

    public decimal QuantityOnHand { get; set; }

    public long UnitCostMinor { get; set; }

    public int AcquiredWorldDay { get; set; }

    public string? Source { get; set; }

    public string? Notes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public StorageLocation? StorageLocation { get; set; }

    public ICollection<InventoryAdjustment> Adjustments { get; set; } = new List<InventoryAdjustment>();
}
