using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/assets")]
public sealed class AssetsController(
    MediaServiceClient mediaServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpGet("{assetId:guid}")]
    public async Task<IActionResult> DownloadAsync(
        Guid assetId,
        [FromQuery] Guid campaignId,
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

        using var response = await mediaServiceClient.ForwardDownloadAsync(
            campaignId,
            assetId,
            authorizationHeader,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(body))
            {
                return StatusCode((int)response.StatusCode);
            }

            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/json",
                Content = body
            };
        }

        Response.ContentType = response.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
        if (response.Content.Headers.ContentLength.HasValue)
        {
            Response.ContentLength = response.Content.Headers.ContentLength.Value;
        }

        if (response.Content.Headers.ContentDisposition is not null)
        {
            Response.Headers.ContentDisposition = response.Content.Headers.ContentDisposition.ToString();
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await stream.CopyToAsync(Response.Body, cancellationToken);

        return new EmptyResult();
    }
}
