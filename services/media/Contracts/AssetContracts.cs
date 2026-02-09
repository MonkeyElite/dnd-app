namespace DndApp.Media.Contracts;

public sealed record CreateAssetUploadRequest(
    string Purpose,
    string FileName,
    string ContentType,
    long? SizeBytes);

public sealed record CreateAssetUploadResponse(
    Guid AssetId,
    string Bucket,
    string ObjectKey,
    string UploadUrl,
    DateTimeOffset ExpiresAt);

public sealed record FinalizeAssetUploadRequest(string? Sha256, long? SizeBytes);

public sealed record FinalizeAssetUploadResponse(bool Ready);

public sealed record AssetDetailsResponse(
    Guid AssetId,
    Guid CampaignId,
    string Purpose,
    string ContentType,
    string Status,
    long? SizeBytes,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record AssetDownloadUrlResponse(string Url, DateTimeOffset ExpiresAt);
