using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/actions")]
public sealed class InventoryActionsController(
    InventoryServiceClient inventoryServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpPost("storage-locations")]
    public async Task<IActionResult> CreateStorageLocationAsync(
        [FromBody] CreateStorageLocationActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await inventoryServiceClient.ForwardCreateStorageLocationAsync(
            request.CampaignId,
            new InventoryCreateStorageLocationRequest(
                request.PlaceId,
                request.Name,
                request.Code,
                request.Type,
                request.Notes),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("inventory/lots")]
    public async Task<IActionResult> CreateInventoryLotAsync(
        [FromBody] CreateInventoryLotActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await inventoryServiceClient.ForwardCreateLotAsync(
            request.CampaignId,
            new InventoryCreateLotRequest(
                request.ItemId,
                request.StorageLocationId,
                request.Quantity,
                request.UnitCostMinor,
                request.AcquiredWorldDay,
                request.Source,
                request.Notes),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("inventory/adjustments")]
    public async Task<IActionResult> CreateInventoryAdjustmentAsync(
        [FromBody] CreateInventoryAdjustmentActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await inventoryServiceClient.ForwardCreateAdjustmentAsync(
            request.CampaignId,
            new InventoryCreateAdjustmentRequest(
                request.ItemId,
                request.StorageLocationId,
                request.LotId,
                request.DeltaQuantity,
                request.Reason,
                request.WorldDay,
                request.Notes,
                request.ReferenceType,
                request.ReferenceId),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }
}
