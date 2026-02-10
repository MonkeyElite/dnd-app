namespace DndApp.Identity.Contracts;

public sealed record UpsertCampaignMembershipRequest(Guid CampaignId, Guid UserId, string Role);

public sealed record CampaignMembershipResponse(Guid CampaignId, Guid UserId, string Role);

public sealed record MyCampaignMembershipResponse(Guid CampaignId, string Role);

public sealed record MyCampaignMemberRoleResponse(Guid CampaignId, Guid UserId, string Role);

public sealed record CampaignMemberSummaryResponse(
    Guid CampaignId,
    Guid UserId,
    string Username,
    string DisplayName,
    string Role,
    bool IsPlatformAdmin);
