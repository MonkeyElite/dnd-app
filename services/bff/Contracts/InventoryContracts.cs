namespace DndApp.Bff.Contracts;

public sealed record InventoryStorageLocationDto(
    Guid StorageLocationId,
    Guid CampaignId,
    Guid? PlaceId,
    string Name,
    string? Code,
    string Type,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record InventoryCreateStorageLocationRequest(
    Guid? PlaceId,
    string Name,
    string? Code,
    string Type,
    string? Notes);

public sealed record InventoryCreateStorageLocationResponse(Guid StorageLocationId);

public sealed record InventoryCreateLotRequest(
    Guid ItemId,
    Guid StorageLocationId,
    decimal Quantity,
    long UnitCostMinor,
    int AcquiredWorldDay,
    string? Source,
    string? Notes);

public sealed record InventoryCreateLotResponse(Guid LotId);

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

public sealed record InventoryCreateAdjustmentRequest(
    Guid ItemId,
    Guid StorageLocationId,
    Guid? LotId,
    decimal DeltaQuantity,
    string Reason,
    int WorldDay,
    string? Notes,
    string? ReferenceType,
    Guid? ReferenceId);

public sealed record InventoryCreateAdjustmentResponse(Guid AdjustmentId);

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

public sealed record InventorySummaryRowDto(Guid ItemId, Guid StorageLocationId, decimal OnHandQuantity);

public sealed record InventorySummaryResponse(IReadOnlyList<InventorySummaryRowDto> Rows);

public sealed record CreateStorageLocationActionRequest(
    Guid CampaignId,
    Guid? PlaceId,
    string Name,
    string? Code,
    string Type,
    string? Notes);

public sealed record CreateInventoryLotActionRequest(
    Guid CampaignId,
    Guid ItemId,
    Guid StorageLocationId,
    decimal Quantity,
    long UnitCostMinor,
    int AcquiredWorldDay,
    string? Source,
    string? Notes);

public sealed record CreateInventoryAdjustmentActionRequest(
    Guid CampaignId,
    Guid ItemId,
    Guid StorageLocationId,
    Guid? LotId,
    decimal DeltaQuantity,
    string Reason,
    int WorldDay,
    string? Notes,
    string? ReferenceType,
    Guid? ReferenceId);

public sealed record InventoryPageFilterPlaceDto(Guid PlaceId, string Name, string Type);

public sealed record InventoryPageFilterStorageLocationDto(
    Guid StorageLocationId,
    Guid? PlaceId,
    string Name,
    string Type,
    string? Code);

public sealed record InventoryPageFiltersDto(
    IReadOnlyList<InventoryPageFilterPlaceDto> Places,
    IReadOnlyList<InventoryPageFilterStorageLocationDto> StorageLocations);

public sealed record InventoryPageImageDto(Guid? AssetId, string? Url);

public sealed record InventoryPageRowDto(
    Guid ItemId,
    string ItemName,
    string CategoryName,
    string UnitName,
    InventoryPageImageDto Image,
    Guid StorageLocationId,
    string StorageLocationName,
    decimal OnHandQuantity);

public sealed record InventoryPageResponse(
    Guid CampaignId,
    string CurrencyCode,
    InventoryPageFiltersDto Filters,
    IReadOnlyList<InventoryPageRowDto> Rows);

public sealed record InventoryLocationsPageRowDto(
    Guid StorageLocationId,
    Guid? PlaceId,
    string? PlaceName,
    string Name,
    string Type,
    string? Code,
    decimal TotalQuantity);

public sealed record InventoryLocationsPageResponse(
    Guid CampaignId,
    IReadOnlyList<InventoryLocationsPageRowDto> Locations);

public sealed record InventoryLocationDetailLotPageRowDto(
    Guid LotId,
    Guid ItemId,
    string ItemName,
    decimal QuantityOnHand,
    long UnitCostMinor,
    int AcquiredWorldDay,
    string? Source,
    string? Notes);

public sealed record InventoryLocationDetailAdjustmentPageRowDto(
    Guid AdjustmentId,
    Guid ItemId,
    string ItemName,
    Guid? LotId,
    decimal DeltaQuantity,
    string Reason,
    int WorldDay,
    string? Notes,
    string? ReferenceType,
    Guid? ReferenceId,
    DateTimeOffset CreatedAt);

public sealed record InventoryLocationDetailPageResponse(
    Guid CampaignId,
    Guid StorageLocationId,
    string StorageLocationName,
    string? StorageLocationCode,
    string StorageLocationType,
    Guid? PlaceId,
    string? PlaceName,
    string CurrencyCode,
    IReadOnlyList<InventoryLocationDetailLotPageRowDto> Lots,
    IReadOnlyList<InventoryLocationDetailAdjustmentPageRowDto> Adjustments);
