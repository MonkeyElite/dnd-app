using System.Text.Json;
using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/actions")]
public sealed class CatalogActionsController(
    CatalogServiceClient catalogServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategoryAsync(
        [FromBody] CreateCategoryActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await catalogServiceClient.ForwardCreateCategoryAsync(
            request.CampaignId,
            new CatalogCreateCategoryRequest(request.Name),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("units")]
    public async Task<IActionResult> CreateUnitAsync(
        [FromBody] CreateUnitActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await catalogServiceClient.ForwardCreateUnitAsync(
            request.CampaignId,
            new CatalogCreateUnitRequest(request.Name),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("tags")]
    public async Task<IActionResult> CreateTagAsync(
        [FromBody] CreateTagActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await catalogServiceClient.ForwardCreateTagAsync(
            request.CampaignId,
            new CatalogCreateTagRequest(request.Name),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("items")]
    public async Task<IActionResult> CreateItemAsync(
        [FromBody] CreateItemActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await catalogServiceClient.ForwardCreateItemAsync(
            request.CampaignId,
            new CatalogCreateItemRequest(
                request.Name,
                request.Description,
                request.CategoryId,
                request.UnitId,
                request.BaseValueMinor,
                request.DefaultListPriceMinor,
                request.Weight,
                request.ImageAssetId,
                request.TagIds),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPut("items/{itemId:guid}")]
    public async Task<IActionResult> UpdateItemAsync(
        Guid itemId,
        [FromBody] JsonElement requestBody,
        CancellationToken cancellationToken)
    {
        var campaignIdError = TryGetCampaignId(requestBody, out var campaignId);
        if (campaignIdError is not null)
        {
            return BadRequest(new ErrorResponse(campaignIdError));
        }

        var permissionResult = await RequireCampaignWrite(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await catalogServiceClient.ForwardUpdateItemAsync(
            campaignId,
            itemId,
            requestBody,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("items/{itemId:guid}/archive")]
    public async Task<IActionResult> SetItemArchiveStateAsync(
        Guid itemId,
        [FromBody] SetItemArchiveActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await catalogServiceClient.ForwardSetItemArchiveStateAsync(
            request.CampaignId,
            itemId,
            new CatalogSetItemArchiveRequest(request.IsArchived),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    private static string? TryGetCampaignId(JsonElement requestBody, out Guid campaignId)
    {
        campaignId = Guid.Empty;

        if (requestBody.ValueKind != JsonValueKind.Object)
        {
            return "request body must be a JSON object.";
        }

        var campaignIdProperty = requestBody
            .EnumerateObject()
            .FirstOrDefault(x => x.Name.Equals("campaignId", StringComparison.OrdinalIgnoreCase));

        if (campaignIdProperty.Equals(default(JsonProperty)))
        {
            return "campaignId is required.";
        }

        if (campaignIdProperty.Value.ValueKind is not JsonValueKind.String
            || !Guid.TryParse(campaignIdProperty.Value.GetString(), out campaignId)
            || campaignId == Guid.Empty)
        {
            return "campaignId must be a non-empty GUID string.";
        }

        return null;
    }
}
