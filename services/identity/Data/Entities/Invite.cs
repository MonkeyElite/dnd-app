namespace DndApp.Identity.Data.Entities;

public sealed class Invite
{
    public Guid InviteId { get; set; }

    public Guid CampaignId { get; set; }

    public string CodeHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public int MaxUses { get; set; } = 1;

    public int Uses { get; set; }

    public DateTimeOffset? ExpiresAt { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
}
