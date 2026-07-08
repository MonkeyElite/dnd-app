using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using DndShop.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/actions")]
public sealed class ActionsController(IdentityServiceClient identityServiceClient) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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

    [HttpGet("invites")]
    public async Task<IActionResult> GetInvitesAsync(
        [FromQuery] Guid campaignId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 20,
        CancellationToken cancellationToken = default)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (skip < 0)
        {
            return BadRequest(new ErrorResponse("skip must be 0 or greater."));
        }

        if (take is < 1 or > 100)
        {
            return BadRequest(new ErrorResponse("take must be between 1 and 100."));
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await identityServiceClient.ForwardGetInvitesAsync(
            campaignId,
            skip,
            take,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("invites/{inviteId:guid}/revoke")]
    public async Task<IActionResult> RevokeInviteAsync(
        Guid inviteId,
        [FromBody] RevokeInviteActionRequest request,
        CancellationToken cancellationToken)
    {
        if (inviteId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("inviteId is required."));
        }

        if (request.CampaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await identityServiceClient.ForwardRevokeInviteAsync(
            request.CampaignId,
            inviteId,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPut("members/{userId:guid}/role")]
    public async Task<IActionResult> UpdateMemberRoleAsync(
        Guid userId,
        [FromBody] UpdateMemberRoleActionRequest request,
        CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("userId is required."));
        }

        if (request.CampaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (!Enum.TryParse<Role>(request.Role?.Trim(), ignoreCase: true, out var role)
            || role is Role.Owner)
        {
            return BadRequest(new ErrorResponse("Role must be one of Admin, Treasurer, Member, or ReadOnly."));
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var membersResponse = await identityServiceClient.ForwardGetCampaignMembersAsync(
            request.CampaignId,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(membersResponse.StatusCode))
        {
            return ToForwardedResult(membersResponse);
        }

        var members = JsonSerializer.Deserialize<List<IdentityCampaignMemberDto>>(membersResponse.Body, JsonOptions);
        if (members is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Identity service returned invalid members JSON."));
        }

        var targetMember = members.SingleOrDefault(member => member.UserId == userId);
        if (targetMember is null)
        {
            return NotFound(new ErrorResponse("Member not found."));
        }

        if (targetMember.IsPlatformAdmin)
        {
            return BadRequest(new ErrorResponse("Platform admin member roles cannot be changed here."));
        }

        if (targetMember.Role.Equals(Role.Owner.ToString(), StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ErrorResponse("Owner roles cannot be changed here."));
        }

        if (TryGetRequestingUserId(out var requestingUserId) && requestingUserId == userId)
        {
            return BadRequest(new ErrorResponse("You cannot change your own role here."));
        }

        var response = await identityServiceClient.ForwardUpsertCampaignMembershipAsync(
            new IdentityUpsertCampaignMembershipRequest(request.CampaignId, userId, role.ToString()),
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

    private static bool IsSuccessStatusCode(int statusCode) => statusCode is >= 200 and <= 299;

    private bool TryGetRequestingUserId(out Guid userId)
    {
        var subjectValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(subjectValue, out userId);
    }
}
