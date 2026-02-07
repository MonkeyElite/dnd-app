using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/auth")]
public sealed class AuthController(IdentityServiceClient identityServiceClient) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await identityServiceClient.ForwardLoginAsync(request, cancellationToken);
        return ToForwardedResult(response);
    }

    [HttpPost("register-with-invite")]
    public async Task<IActionResult> RegisterWithInviteAsync(
        [FromBody] RegisterWithInviteRequest request,
        CancellationToken cancellationToken)
    {
        var response = await identityServiceClient.ForwardRegisterWithInviteAsync(request, cancellationToken);
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
