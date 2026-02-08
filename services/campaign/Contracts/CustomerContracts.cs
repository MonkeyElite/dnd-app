namespace DndApp.Campaign.Contracts;

public sealed record CreateCustomerRequest(string Name, string? Notes, IReadOnlyList<string>? Tags);

public sealed record CreateCustomerResponse(Guid CustomerId);

public sealed record CustomerDto(Guid CustomerId, Guid CampaignId, string Name, string? Notes, IReadOnlyList<string> Tags);
