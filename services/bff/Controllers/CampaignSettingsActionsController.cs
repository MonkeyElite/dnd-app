using System.Text.Json;
using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/actions/campaign-settings")]
public sealed class CampaignSettingsActionsController(
    CampaignServiceClient campaignServiceClient,
    IdentityServiceClient identityServiceClient) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPut("calendar")]
    public async Task<IActionResult> UpdateCalendarAsync(
        [FromBody] UpdateCalendarSettingsActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CampaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (request.Calendar is null)
        {
            return BadRequest(new ErrorResponse("calendar is required."));
        }

        if (request.Calendar.CampaignId != Guid.Empty && request.Calendar.CampaignId != request.CampaignId)
        {
            return BadRequest(new ErrorResponse("calendar.campaignId must match campaignId."));
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var permissionResult = await EnsureCanManageSettingsAsync(request.CampaignId, authorizationHeader, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var downstreamRequest = new CampaignCalendarUpdateRequest(request.Calendar.WeekLength, request.Calendar.Months);
        var response = await campaignServiceClient.ForwardUpdateCalendarSettingsAsync(
            request.CampaignId,
            downstreamRequest,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPut("currency")]
    public async Task<IActionResult> UpdateCurrencyAsync(
        [FromBody] UpdateCurrencySettingsActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.CampaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        if (request.Currency is null)
        {
            return BadRequest(new ErrorResponse("currency is required."));
        }

        if (request.Currency.CampaignId != Guid.Empty && request.Currency.CampaignId != request.CampaignId)
        {
            return BadRequest(new ErrorResponse("currency.campaignId must match campaignId."));
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var permissionResult = await EnsureCanManageSettingsAsync(request.CampaignId, authorizationHeader, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var downstreamRequest = new CampaignCurrencyUpdateRequest(
            request.Currency.CurrencyCode,
            request.Currency.MinorUnitName,
            request.Currency.MajorUnitName,
            request.Currency.Denominations);

        var response = await campaignServiceClient.ForwardUpdateCurrencySettingsAsync(
            request.CampaignId,
            downstreamRequest,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    private async Task<IActionResult?> EnsureCanManageSettingsAsync(
        Guid campaignId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        if (IsPlatformAdmin())
        {
            return null;
        }

        var membershipResponse = await identityServiceClient.ForwardGetMyCampaignMembershipForCampaignAsync(
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

        if (!CanManageSettingsByRole(membership.Role))
        {
            return Forbid();
        }

        return null;
    }

    private bool IsPlatformAdmin()
    {
        var claimValue = User.FindFirst("isPlatformAdmin")?.Value;
        return bool.TryParse(claimValue, out var isPlatformAdmin) && isPlatformAdmin;
    }

    private static bool CanManageSettingsByRole(string role)
        => role.Equals("Owner", StringComparison.OrdinalIgnoreCase)
            || role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

    private static T? DeserializeBody<T>(string body)
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

    private static bool IsSuccessStatusCode(int statusCode)
        => statusCode is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices;

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
