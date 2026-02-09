namespace DndApp.Bff.Contracts;

public sealed record CreateAssetUploadActionRequest(
    Guid CampaignId,
    string Purpose,
    string FileName,
    string ContentType,
    long? SizeBytes);

public sealed record FinalizeAssetUploadActionRequest(Guid CampaignId, string? Sha256, long? SizeBytes);

public sealed record MediaCreateAssetUploadRequest(
    string Purpose,
    string FileName,
    string ContentType,
    long? SizeBytes);

public sealed record MediaFinalizeAssetUploadRequest(string? Sha256, long? SizeBytes);

public sealed record MediaDownloadUrlResponse(string Url, DateTimeOffset ExpiresAt);
