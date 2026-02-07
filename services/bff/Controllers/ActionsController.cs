using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/actions")]
public sealed class ActionsController(IdentityServiceClient identityServiceClient) : ControllerBase
{
    [HttpPost("invites")]
    public async Task<IActionResult> CreateInviteAsync(
        [FromBody] CreateInviteActionRequest request,
        CancellationToken cancellationToken)
    {
        var identityRequest = new IdentityCreateInviteRequest(request.Role, request.MaxUses, request.ExpiresInDays);
        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await identityServiceClient.ForwardCreateInviteAsync(
            request.CampaignId,
            identityRequest,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    private IActionResult ToForwardedResult(ForwardedJsonResponse response)
    {
        if (string.IsNullOrEmpty(response.Body))
        {
            return StatusCode(response.StatusCode);
        }

        return new ContentResult
        {
            StatusCode = response.StatusCode,
            ContentType = response.ContentType,
            Content = response.Body
        };
    }
}
