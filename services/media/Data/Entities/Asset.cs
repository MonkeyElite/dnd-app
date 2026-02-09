namespace DndApp.Media.Data.Entities;

public sealed class Asset
{
    public Guid AssetId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid OwnerUserId { get; set; }

    public string Purpose { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string? OriginalFileName { get; set; }

    public long? SizeBytes { get; set; }

    public string Bucket { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public string? Sha256 { get; set; }

    public string Status { get; set; } = AssetStatuses.PendingUpload;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}
