using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pages")]
public sealed class PagesController(
    CampaignServiceClient campaignServiceClient,
    IdentityServiceClient identityServiceClient) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    [HttpPost("campaigns")]
    public async Task<IActionResult> CreateCampaignAsync(
        [FromBody] CreateCampaignRequest request,
        CancellationToken cancellationToken)
    {
        if (!IsPlatformAdmin())
        {
            return Forbid();
        }

        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();

        var createCampaignResponse = await campaignServiceClient.ForwardCreateCampaignAsync(
            request,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(createCampaignResponse.StatusCode))
        {
            return ToForwardedResult(createCampaignResponse);
        }

        var createCampaignPayload = DeserializeBody<CreateCampaignResponse>(createCampaignResponse.Body);
        if (createCampaignPayload is null)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new ErrorResponse("Campaign service returned invalid JSON."));
        }

        var upsertMembershipResponse = await identityServiceClient.ForwardUpsertCampaignMembershipAsync(
            new IdentityUpsertCampaignMembershipRequest(createCampaignPayload.CampaignId, requestingUserId, "Owner"),
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(upsertMembershipResponse.StatusCode))
        {
            return ToForwardedResult(upsertMembershipResponse);
        }

        return StatusCode(createCampaignResponse.StatusCode, new CreateCampaignResponse(createCampaignPayload.CampaignId));
    }

    [HttpGet("campaigns")]
    public async Task<IActionResult> GetCampaignsAsync(CancellationToken cancellationToken)
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();

        var membershipsResponse = await identityServiceClient.ForwardGetMyCampaignMembershipsAsync(
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(membershipsResponse.StatusCode))
        {
            return ToForwardedResult(membershipsResponse);
        }

        var memberships = DeserializeBody<List<IdentityCampaignMembershipDto>>(membershipsResponse.Body) ?? [];
        if (memberships.Count == 0)
        {
            return Ok(new CampaignsPageResponse([]));
        }

        var campaignTasks = memberships
            .Select(x => campaignServiceClient.ForwardGetCampaignAsync(x.CampaignId, authorizationHeader, cancellationToken))
            .ToList();

        var campaignResponses = await Task.WhenAll(campaignTasks);

        var campaigns = new List<CampaignSummaryDto>();
        for (var index = 0; index < memberships.Count; index++)
        {
            var membership = memberships[index];
            var campaignResponse = campaignResponses[index];

            if (campaignResponse.StatusCode == StatusCodes.Status404NotFound)
            {
                continue;
            }

            if (!IsSuccessStatusCode(campaignResponse.StatusCode))
            {
                return ToForwardedResult(campaignResponse);
            }

            var campaign = DeserializeBody<CampaignDetailsDto>(campaignResponse.Body);
            if (campaign is null)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    new ErrorResponse("Campaign service returned invalid JSON."));
            }

            campaigns.Add(new CampaignSummaryDto(campaign.CampaignId, campaign.Name, membership.Role));
        }

        var sortedCampaigns = campaigns
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return Ok(new CampaignsPageResponse(sortedCampaigns));
    }

    [HttpGet("campaign-settings")]
    public async Task<IActionResult> GetCampaignSettingsAsync([FromQuery] Guid campaignId, CancellationToken cancellationToken)
    {
        if (campaignId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("campaignId is required."));
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();

        var membershipResponse = await identityServiceClient.ForwardGetMyCampaignMembershipForCampaignAsync(
            campaignId,
            authorizationHeader,
            cancellationToken);

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

        var calendarTask = campaignServiceClient.ForwardGetCalendarSettingsAsync(campaignId, authorizationHeader, cancellationToken);
        var currencyTask = campaignServiceClient.ForwardGetCurrencySettingsAsync(campaignId, authorizationHeader, cancellationToken);
        await Task.WhenAll(calendarTask, currencyTask);

        if (!IsSuccessStatusCode(calendarTask.Result.StatusCode))
        {
            return ToForwardedResult(calendarTask.Result);
        }

        if (!IsSuccessStatusCode(currencyTask.Result.StatusCode))
        {
            return ToForwardedResult(currencyTask.Result);
        }

        var calendar = DeserializeBody<CalendarConfigDto>(calendarTask.Result.Body);
        if (calendar is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid calendar JSON."));
        }

        var currency = DeserializeBody<CurrencyConfigDto>(currencyTask.Result.Body);
        if (currency is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid currency JSON."));
        }

        return Ok(new CampaignSettingsPageResponse(campaignId, membership.Role, calendar, currency));
    }

    private bool IsPlatformAdmin()
    {
        var claimValue = User.FindFirst("isPlatformAdmin")?.Value;
        return bool.TryParse(claimValue, out var isPlatformAdmin) && isPlatformAdmin;
    }

    private bool TryGetRequestingUserId(out Guid userId)
    {
        var subjectValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(subjectValue, out userId);
    }

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
