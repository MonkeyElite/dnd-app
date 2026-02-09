namespace DndApp.Media.Services;

public sealed record StoredObjectInfo(long SizeBytes, string? ContentType);

public interface IMediaObjectStorage
{
    string BucketName { get; }

    Task EnsureBucketExistsAsync(CancellationToken cancellationToken);

    Task<string> CreatePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        int expiresInSeconds,
        bool rewriteToPublicBaseUrl,
        CancellationToken cancellationToken);

    Task<string> CreatePresignedDownloadUrlAsync(
        string objectKey,
        int expiresInSeconds,
        bool rewriteToPublicBaseUrl,
        CancellationToken cancellationToken);

    Task<StoredObjectInfo?> TryGetObjectInfoAsync(string objectKey, CancellationToken cancellationToken);

    Task<HttpResponseMessage> GetObjectProxyResponseAsync(string objectKey, CancellationToken cancellationToken);
}
