namespace DndApp.Identity.Data.Entities;

public sealed class CampaignMembership
{
    public Guid CampaignId { get; set; }

    public Guid UserId { get; set; }

    public string Role { get; set; } = string.Empty;

    public User? User { get; set; }
}
