namespace DndApp.Identity.Contracts;

public sealed record CreateInviteRequest(string Role, int MaxUses = 1, int? ExpiresInDays = null);

public sealed record CreateInviteResponse(Guid InviteId, string Code, string Role, int MaxUses, DateTimeOffset? ExpiresAt);

public sealed record InviteSummaryResponse(
    Guid InviteId,
    string Role,
    int Uses,
    int MaxUses,
    DateTimeOffset? ExpiresAt,
    DateTimeOffset? RevokedAt,
    DateTimeOffset CreatedAt);

public sealed record RevokeInviteResponse(bool Revoked);
