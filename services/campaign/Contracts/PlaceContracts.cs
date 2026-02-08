namespace DndApp.Campaign.Contracts;

public sealed record CreatePlaceRequest(string Name, string Type, string? Notes);

public sealed record CreatePlaceResponse(Guid PlaceId);

public sealed record PlaceDto(Guid PlaceId, Guid CampaignId, string Name, string Type, string? Notes);
