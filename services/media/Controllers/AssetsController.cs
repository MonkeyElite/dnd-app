using System.Text.RegularExpressions;
using DndApp.Media.Contracts;
using DndApp.Media.Data;
using DndApp.Media.Data.Entities;
using DndApp.Media.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Media.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/assets")]
public sealed class AssetsController(
    MediaDbContext dbContext,
    IMediaObjectStorage mediaObjectStorage,
    ILogger<AssetsController> logger) : MediaControllerBase
{
    private const int UploadUrlExpirySeconds = 15 * 60;
    private const int DefaultDownloadUrlExpirySeconds = 60 * 60;
    private const int MaxDownloadUrlExpirySeconds = 24 * 60 * 60;
    private static readonly Regex Sha256Regex = new("^[0-9a-fA-F]{64}$", RegexOptions.Compiled);
    private static readonly Regex FileNameAllowedCharactersRegex = new("[^A-Za-z0-9._-]+", RegexOptions.Compiled);

    [HttpPost("uploads")]
    public async Task<IActionResult> CreateUploadAsync(
        Guid campaignId,
        [FromBody] CreateAssetUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (!TryGetRequestingUserId(out var ownerUserId))
        {
            return Unauthorized();
        }

        var validationError = ValidateCreateUploadRequest(request, out var normalizedRequest);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var assetId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var asset = new Asset
        {
            AssetId = assetId,
            CampaignId = campaignId,
            OwnerUserId = ownerUserId,
            Purpose = normalizedRequest.Purpose,
            ContentType = normalizedRequest.ContentType,
            OriginalFileName = normalizedRequest.FileName,
            SizeBytes = normalizedRequest.SizeBytes,
            Bucket = mediaObjectStorage.BucketName,
            ObjectKey = BuildObjectKey(campaignId, assetId, normalizedRequest.FileName),
            Status = AssetStatuses.PendingUpload,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Assets.Add(asset);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await mediaObjectStorage.EnsureBucketExistsAsync(cancellationToken);

            var uploadUrl = await mediaObjectStorage.CreatePresignedUploadUrlAsync(
                asset.ObjectKey,
                asset.ContentType,
                UploadUrlExpirySeconds,
                rewriteToPublicBaseUrl: true,
                cancellationToken);

            return Ok(new CreateAssetUploadResponse(
                asset.AssetId,
                asset.Bucket,
                asset.ObjectKey,
                uploadUrl,
                DateTimeOffset.UtcNow.AddSeconds(UploadUrlExpirySeconds)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to create upload URL for asset {AssetId}.", asset.AssetId);

            asset.Status = AssetStatuses.Failed;
            asset.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse("Failed to create upload URL."));
        }
    }

    [HttpPost("{assetId:guid}/finalize")]
    public async Task<IActionResult> FinalizeUploadAsync(
        Guid campaignId,
        Guid assetId,
        [FromBody] FinalizeAssetUploadRequest request,
        CancellationToken cancellationToken)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (assetId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("assetId is required."));
        }

        var validationError = ValidateFinalizeRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var asset = await dbContext.Assets
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.AssetId == assetId, cancellationToken);

        if (asset is null)
        {
            return NotFound(new ErrorResponse("Asset not found."));
        }

        StoredObjectInfo? objectInfo;
        try
        {
            objectInfo = await mediaObjectStorage.TryGetObjectInfoAsync(asset.ObjectKey, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to verify storage object for asset {AssetId}.", asset.AssetId);
            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse("Failed to verify uploaded object."));
        }

        if (objectInfo is null)
        {
            asset.Status = AssetStatuses.Failed;
            asset.UpdatedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);

            return BadRequest(new ErrorResponse("Uploaded object not found in storage."));
        }

        asset.Status = AssetStatuses.Ready;
        asset.SizeBytes = request.SizeBytes ?? objectInfo.SizeBytes;
        asset.Sha256 = NormalizeSha256(request.Sha256);
        asset.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new FinalizeAssetUploadResponse(true));
    }

    [HttpGet("{assetId:guid}")]
    public async Task<IActionResult> GetAssetAsync(Guid campaignId, Guid assetId, CancellationToken cancellationToken)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (assetId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("assetId is required."));
        }

        var asset = await dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.AssetId == assetId, cancellationToken);

        if (asset is null)
        {
            return NotFound(new ErrorResponse("Asset not found."));
        }

        return Ok(new AssetDetailsResponse(
            asset.AssetId,
            asset.CampaignId,
            asset.Purpose,
            asset.ContentType,
            asset.Status,
            asset.SizeBytes,
            asset.CreatedAt,
            asset.UpdatedAt));
    }

    [HttpGet("{assetId:guid}/download-url")]
    public async Task<IActionResult> GetDownloadUrlAsync(
        Guid campaignId,
        Guid assetId,
        [FromQuery] int? expiresInSeconds,
        CancellationToken cancellationToken)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (assetId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("assetId is required."));
        }

        var normalizedExpirySeconds = NormalizeDownloadUrlExpiry(expiresInSeconds);
        if (normalizedExpirySeconds is null)
        {
            return BadRequest(new ErrorResponse($"expiresInSeconds must be between 1 and {MaxDownloadUrlExpirySeconds}."));
        }

        var asset = await dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.AssetId == assetId, cancellationToken);

        if (asset is null)
        {
            return NotFound(new ErrorResponse("Asset not found."));
        }

        if (!string.Equals(asset.Status, AssetStatuses.Ready, StringComparison.Ordinal))
        {
            return BadRequest(new ErrorResponse("Asset is not ready."));
        }

        try
        {
            var url = await mediaObjectStorage.CreatePresignedDownloadUrlAsync(
                asset.ObjectKey,
                normalizedExpirySeconds.Value,
                rewriteToPublicBaseUrl: true,
                cancellationToken);

            return Ok(new AssetDownloadUrlResponse(
                url,
                DateTimeOffset.UtcNow.AddSeconds(normalizedExpirySeconds.Value)));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Failed to create download URL for asset {AssetId}.", asset.AssetId);
            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse("Failed to create download URL."));
        }
    }

    [HttpGet("{assetId:guid}/download")]
    public async Task<IActionResult> DownloadAsync(Guid campaignId, Guid assetId, CancellationToken cancellationToken)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (assetId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("assetId is required."));
        }

        var asset = await dbContext.Assets
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.AssetId == assetId, cancellationToken);

        if (asset is null)
        {
            return NotFound(new ErrorResponse("Asset not found."));
        }

        if (!string.Equals(asset.Status, AssetStatuses.Ready, StringComparison.Ordinal))
        {
            return BadRequest(new ErrorResponse("Asset is not ready."));
        }

        using var storageResponse = await mediaObjectStorage.GetObjectProxyResponseAsync(asset.ObjectKey, cancellationToken);
        if (!storageResponse.IsSuccessStatusCode)
        {
            if ((int)storageResponse.StatusCode == StatusCodes.Status404NotFound)
            {
                return NotFound(new ErrorResponse("Asset object not found in storage."));
            }

            logger.LogWarning(
                "Storage proxy download failed for asset {AssetId} with status code {StatusCode}.",
                asset.AssetId,
                (int)storageResponse.StatusCode);

            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse("Failed to stream asset from storage."));
        }

        Response.ContentType = asset.ContentType;
        if (storageResponse.Content.Headers.ContentLength.HasValue)
        {
            Response.ContentLength = storageResponse.Content.Headers.ContentLength.Value;
        }

        Response.Headers.CacheControl = "private, max-age=60";

        await using var stream = await storageResponse.Content.ReadAsStreamAsync(cancellationToken);
        await stream.CopyToAsync(Response.Body, cancellationToken);
        return new EmptyResult();
    }

    private static string? ValidateCreateUploadRequest(
        CreateAssetUploadRequest request,
        out NormalizedCreateUploadRequest normalizedRequest)
    {
        var purpose = request.Purpose?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(purpose))
        {
            normalizedRequest = default;
            return "purpose is required.";
        }

        if (purpose.Length > 100)
        {
            normalizedRequest = default;
            return "purpose must be 100 characters or fewer.";
        }

        var fileName = request.FileName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fileName))
        {
            normalizedRequest = default;
            return "fileName is required.";
        }

        if (fileName.Length > 200)
        {
            normalizedRequest = default;
            return "fileName must be 200 characters or fewer.";
        }

        var contentType = request.ContentType?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            normalizedRequest = default;
            return "contentType is required.";
        }

        if (contentType.Length > 200)
        {
            normalizedRequest = default;
            return "contentType must be 200 characters or fewer.";
        }

        if (request.SizeBytes is < 0)
        {
            normalizedRequest = default;
            return "sizeBytes must be greater than or equal to 0 when provided.";
        }

        normalizedRequest = new NormalizedCreateUploadRequest(
            purpose,
            fileName,
            contentType,
            request.SizeBytes);

        return null;
    }

    private static string? ValidateFinalizeRequest(FinalizeAssetUploadRequest request)
    {
        if (request.SizeBytes is < 0)
        {
            return "sizeBytes must be greater than or equal to 0 when provided.";
        }

        if (request.Sha256 is not null)
        {
            var normalized = request.Sha256.Trim();
            if (!Sha256Regex.IsMatch(normalized))
            {
                return "sha256 must be a 64-character hexadecimal string when provided.";
            }
        }

        return null;
    }

    private static int? NormalizeDownloadUrlExpiry(int? expiresInSeconds)
    {
        if (!expiresInSeconds.HasValue)
        {
            return DefaultDownloadUrlExpirySeconds;
        }

        if (expiresInSeconds <= 0 || expiresInSeconds > MaxDownloadUrlExpirySeconds)
        {
            return null;
        }

        return expiresInSeconds.Value;
    }

    private static string? NormalizeSha256(string? sha256)
        => string.IsNullOrWhiteSpace(sha256) ? null : sha256.Trim().ToLowerInvariant();

    private static string BuildObjectKey(Guid campaignId, Guid assetId, string fileName)
        => $"campaigns/{campaignId:D}/assets/{assetId:D}/{SanitizeFileName(fileName)}";

    private static string SanitizeFileName(string fileName)
    {
        var leafName = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(leafName))
        {
            return "file";
        }

        var sanitized = FileNameAllowedCharactersRegex.Replace(leafName, "-");
        sanitized = sanitized.Trim('-', '.', '_');
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "file";
        }

        if (sanitized.Length > 200)
        {
            sanitized = sanitized[..200];
        }

        return sanitized;
    }

    private readonly record struct NormalizedCreateUploadRequest(
        string Purpose,
        string FileName,
        string ContentType,
        long? SizeBytes);
}
