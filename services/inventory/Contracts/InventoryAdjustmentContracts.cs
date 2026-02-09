namespace DndApp.Inventory.Contracts;

public sealed record CreateInventoryAdjustmentRequest(
    Guid ItemId,
    Guid StorageLocationId,
    Guid? LotId,
    decimal DeltaQuantity,
    string Reason,
    int WorldDay,
    string? Notes,
    string? ReferenceType,
    Guid? ReferenceId);

public sealed record CreateInventoryAdjustmentResponse(Guid AdjustmentId);

public sealed record InventoryAdjustmentDto(
    Guid AdjustmentId,
    Guid ItemId,
    Guid StorageLocationId,
    Guid? LotId,
    decimal DeltaQuantity,
    string Reason,
    int WorldDay,
    string? Notes,
    string? ReferenceType,
    Guid? ReferenceId,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt);
