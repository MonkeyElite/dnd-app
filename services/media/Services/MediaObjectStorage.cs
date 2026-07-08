using System.Net.Http.Headers;
using DndApp.Media.Options;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace DndApp.Media.Services;

public sealed class MediaObjectStorage(
    IMinioClient minioClient,
    IHttpClientFactory httpClientFactory,
    IOptions<S3StorageOptions> options,
    ILogger<MediaObjectStorage> logger) : IMediaObjectStorage
{
    private readonly IMinioClient _minioClient = minioClient;
    private readonly IMinioClient _publicPresignMinioClient = CreatePublicPresignClient(options.Value) ?? minioClient;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly S3StorageOptions _options = options.Value;
    private readonly ILogger<MediaObjectStorage> _logger = logger;
    private readonly SemaphoreSlim _bucketInitLock = new(1, 1);
    private volatile bool _bucketInitialized;

    public string BucketName => _options.Bucket;

    public async Task EnsureBucketExistsAsync(CancellationToken cancellationToken)
    {
        if (_bucketInitialized)
        {
            return;
        }

        await _bucketInitLock.WaitAsync(cancellationToken);
        try
        {
            if (_bucketInitialized)
            {
                return;
            }

            var bucketExists = await _minioClient
                .BucketExistsAsync(new BucketExistsArgs().WithBucket(_options.Bucket))
                .ConfigureAwait(false);

            if (!bucketExists)
            {
                var makeBucketArgs = new MakeBucketArgs().WithBucket(_options.Bucket);
                await _minioClient.MakeBucketAsync(makeBucketArgs).ConfigureAwait(false);
                _logger.LogInformation("Created bucket {Bucket}.", _options.Bucket);
            }

            _bucketInitialized = true;
        }
        finally
        {
            _bucketInitLock.Release();
        }
    }

    public async Task<string> CreatePresignedUploadUrlAsync(
        string objectKey,
        string contentType,
        int expiresInSeconds,
        bool rewriteToPublicBaseUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var requestHeaders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Content-Type"] = contentType
        };

        var presignClient = rewriteToPublicBaseUrl ? _publicPresignMinioClient : _minioClient;
        return await presignClient
            .PresignedPutObjectAsync(new PresignedPutObjectArgs()
                .WithBucket(_options.Bucket)
                .WithObject(objectKey)
                .WithExpiry(expiresInSeconds)
                .WithHeaders(requestHeaders))
            .ConfigureAwait(false);
    }

    public async Task<string> CreatePresignedDownloadUrlAsync(
        string objectKey,
        int expiresInSeconds,
        bool rewriteToPublicBaseUrl,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var presignClient = rewriteToPublicBaseUrl ? _publicPresignMinioClient : _minioClient;
        return await presignClient
            .PresignedGetObjectAsync(new PresignedGetObjectArgs()
                .WithBucket(_options.Bucket)
                .WithObject(objectKey)
                .WithExpiry(expiresInSeconds))
            .ConfigureAwait(false);
    }

    public async Task<StoredObjectInfo?> TryGetObjectInfoAsync(string objectKey, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var stat = await _minioClient
                .StatObjectAsync(new StatObjectArgs()
                    .WithBucket(_options.Bucket)
                    .WithObject(objectKey))
                .ConfigureAwait(false);

            var contentType = string.IsNullOrWhiteSpace(stat.ContentType) ? null : stat.ContentType;
            var sizeBytes = Convert.ToInt64(stat.Size);
            return new StoredObjectInfo(sizeBytes, contentType);
        }
        catch (Exception exception) when (IsNotFound(exception))
        {
            return null;
        }
    }

    public async Task<HttpResponseMessage> GetObjectProxyResponseAsync(
        string objectKey,
        CancellationToken cancellationToken)
    {
        var url = await CreatePresignedDownloadUrlAsync(
            objectKey,
            expiresInSeconds: 60,
            rewriteToPublicBaseUrl: false,
            cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        var response = await _httpClientFactory
            .CreateClient()
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return response;
        }

        response.Content.Headers.ContentType ??= new MediaTypeHeaderValue("application/octet-stream");
        return response;
    }

    private static IMinioClient? CreatePublicPresignClient(S3StorageOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.PublicBaseUrl))
        {
            return null;
        }

        if (!Uri.TryCreate(options.PublicBaseUrl, UriKind.Absolute, out var publicBaseUri))
        {
            return null;
        }

        var endpoint = publicBaseUri.IsDefaultPort
            ? publicBaseUri.Host
            : $"{publicBaseUri.Host}:{publicBaseUri.Port}";

        var minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(options.AccessKey, options.SecretKey)
            .WithSSL(string.Equals(publicBaseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            .Build();

        if (!string.IsNullOrWhiteSpace(options.Region))
        {
            minioClient = minioClient.WithRegion(options.Region);
        }

        return minioClient;
    }

    private static bool IsNotFound(Exception exception)
    {
        var typeName = exception.GetType().Name;
        if (typeName.Contains("ObjectNotFound", StringComparison.OrdinalIgnoreCase)
            || typeName.Contains("BucketNotFound", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var message = exception.Message;
        return message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            || message.Contains("not exist", StringComparison.OrdinalIgnoreCase)
            || message.Contains("NoSuchKey", StringComparison.OrdinalIgnoreCase)
            || message.Contains("NoSuchBucket", StringComparison.OrdinalIgnoreCase);
    }
}
