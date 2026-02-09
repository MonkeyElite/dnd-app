namespace DndApp.Inventory.Contracts;

public sealed record CreateInventoryLotRequest(
    Guid ItemId,
    Guid StorageLocationId,
    decimal Quantity,
    long UnitCostMinor,
    int AcquiredWorldDay,
    string? Source,
    string? Notes);

public sealed record CreateInventoryLotResponse(Guid LotId);

public sealed record InventoryLotDto(
    Guid LotId,
    Guid ItemId,
    Guid StorageLocationId,
    decimal QuantityOnHand,
    long UnitCostMinor,
    int AcquiredWorldDay,
    string? Source,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
