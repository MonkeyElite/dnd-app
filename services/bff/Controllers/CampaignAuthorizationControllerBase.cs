using System.Text.Json;
using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using DndShop.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

public abstract class CampaignAuthorizationControllerBase(IdentityServiceClient identityServiceClient) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IdentityServiceClient _identityServiceClient = identityServiceClient;

    protected async Task<IActionResult?> RequireCampaignRead(Guid campaignId, CancellationToken cancellationToken)
        => await RequireCampaignRoleAsync(campaignId, CanReadRole, cancellationToken);

    protected async Task<IActionResult?> RequireCampaignWrite(Guid campaignId, CancellationToken cancellationToken)
        => await RequireCampaignRoleAsync(campaignId, CanWriteRole, cancellationToken);

    protected bool IsPlatformAdmin()
    {
        var claimValue = User.FindFirst("isPlatformAdmin")?.Value;
        return bool.TryParse(claimValue, out var isPlatformAdmin) && isPlatformAdmin;
    }

    protected static bool IsSuccessStatusCode(int statusCode)
        => statusCode is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices;

    protected static T? DeserializeBody<T>(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(body, JsonOptions);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    protected IActionResult ToForwardedResult(ForwardedJsonResponse response)
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

    private async Task<IActionResult?> RequireCampaignRoleAsync(
        Guid campaignId,
        Func<Role, bool> rolePredicate,
        CancellationToken cancellationToken)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (IsPlatformAdmin())
        {
            return null;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var membershipResponse = await _identityServiceClient.ForwardGetMyCampaignMembershipForCampaignAsync(
            campaignId,
            authorizationHeader,
            cancellationToken);

        if (membershipResponse.StatusCode == StatusCodes.Status404NotFound)
        {
            return Forbid();
        }

        if (!IsSuccessStatusCode(membershipResponse.StatusCode))
        {
            return ToForwardedResult(membershipResponse);
        }

        var membership = DeserializeBody<IdentityCampaignMemberMeDto>(membershipResponse.Body);
        if (membership is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Identity service returned invalid JSON."));
        }

        if (!Enum.TryParse<Role>(membership.Role?.Trim(), ignoreCase: true, out var role)
            || !rolePredicate(role))
        {
            return Forbid();
        }

        return null;
    }

    private static bool CanReadRole(Role role)
        => role is Role.Owner
            or Role.Admin
            or Role.Treasurer
            or Role.Member
            or Role.ReadOnly;

    private static bool CanWriteRole(Role role)
        => role is Role.Owner
            or Role.Admin
            or Role.Treasurer;
}
