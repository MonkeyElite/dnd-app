namespace DndApp.Bff.Contracts;

public sealed record CreateInviteActionRequest(Guid CampaignId, string Role, int MaxUses = 1, int? ExpiresInDays = null);

public sealed record IdentityCreateInviteRequest(string Role, int MaxUses = 1, int? ExpiresInDays = null);
