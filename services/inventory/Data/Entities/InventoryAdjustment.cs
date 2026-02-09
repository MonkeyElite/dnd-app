namespace DndApp.Inventory.Data.Entities;

public sealed class InventoryAdjustment
{
    public Guid AdjustmentId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid ItemId { get; set; }

    public Guid StorageLocationId { get; set; }

    public Guid? LotId { get; set; }

    public decimal DeltaQuantity { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int WorldDay { get; set; }

    public string? Notes { get; set; }

    public string? ReferenceType { get; set; }

    public Guid? ReferenceId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public StorageLocation? StorageLocation { get; set; }

    public InventoryLot? Lot { get; set; }
}
