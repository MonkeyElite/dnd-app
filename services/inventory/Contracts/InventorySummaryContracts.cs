namespace DndApp.Inventory.Contracts;

public sealed record InventorySummaryRowDto(Guid ItemId, Guid StorageLocationId, decimal OnHandQuantity);

public sealed record InventorySummaryResponse(IReadOnlyList<InventorySummaryRowDto> Rows);
