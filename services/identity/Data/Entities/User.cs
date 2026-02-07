namespace DndApp.Identity.Data.Entities;

public sealed class User
{
    public Guid UserId { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public ICollection<CampaignMembership> CampaignMemberships { get; set; } = new List<CampaignMembership>();
}
