using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/actions")]
public sealed class AssetsActionsController(
    MediaServiceClient mediaServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpPost("assets/uploads")]
    public async Task<IActionResult> CreateUploadAsync(
        [FromBody] CreateAssetUploadActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await mediaServiceClient.ForwardCreateUploadAsync(
            request.CampaignId,
            new MediaCreateAssetUploadRequest(
                request.Purpose,
                request.FileName,
                request.ContentType,
                request.SizeBytes),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("assets/{assetId:guid}/finalize")]
    public async Task<IActionResult> FinalizeUploadAsync(
        Guid assetId,
        [FromBody] FinalizeAssetUploadActionRequest request,
        CancellationToken cancellationToken)
    {
        if (assetId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("assetId is required."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await mediaServiceClient.ForwardFinalizeUploadAsync(
            request.CampaignId,
            assetId,
            new MediaFinalizeAssetUploadRequest(request.Sha256, request.SizeBytes),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpGet("assets/{assetId:guid}/download-url")]
    public async Task<IActionResult> GetDownloadUrlAsync(
        Guid assetId,
        [FromQuery] Guid campaignId,
        [FromQuery] int? expiresInSeconds,
        CancellationToken cancellationToken)
    {
        if (assetId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("assetId is required."));
        }

        var permissionResult = await RequireCampaignRead(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await mediaServiceClient.ForwardGetDownloadUrlAsync(
            campaignId,
            assetId,
            expiresInSeconds,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }
}
