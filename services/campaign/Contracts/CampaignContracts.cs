namespace DndApp.Campaign.Contracts;

public sealed record CreateCampaignRequest(string Name, string? Description);

public sealed record CreateCampaignResponse(Guid CampaignId);

public sealed record CampaignResponse(
    Guid CampaignId,
    string Name,
    string? Description,
    Guid CreatedByUserId,
    DateTimeOffset CreatedAt);

public sealed record UpdateResultResponse(bool Updated);
