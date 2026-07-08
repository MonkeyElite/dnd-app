namespace DndApp.Bff.Contracts;

public sealed record CreateInviteActionRequest(Guid CampaignId, string Role, int MaxUses = 1, int? ExpiresInDays = null);

public sealed record IdentityCreateInviteRequest(string Role, int MaxUses = 1, int? ExpiresInDays = null);

public sealed record InviteSummaryDto(
    Guid InviteId,
    string Role,
    int Uses,
    int MaxUses,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt);

public sealed record InviteSummaryPageDto(
    IReadOnlyList<InviteSummaryDto> Items,
    int TotalCount,
    int Skip,
    int Take);

public sealed record RevokeInviteActionRequest(Guid CampaignId);

public sealed record RevokeInviteResponse(bool Revoked);

public sealed record UpdateMemberRoleActionRequest(Guid CampaignId, string Role);
