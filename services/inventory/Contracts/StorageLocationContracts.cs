namespace DndApp.Inventory.Contracts;

public sealed record StorageLocationDto(
    Guid StorageLocationId,
    Guid CampaignId,
    Guid? PlaceId,
    string Name,
    string? Code,
    string Type,
    string? Notes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateStorageLocationRequest(
    Guid? PlaceId,
    string Name,
    string? Code,
    string Type,
    string? Notes);

public sealed record CreateStorageLocationResponse(Guid StorageLocationId);
