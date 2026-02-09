using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/actions")]
public sealed class SalesActionsController(
    SalesServiceClient salesServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpPost("sales")]
    public async Task<IActionResult> CreateSaleAsync(
        [FromBody] CreateSaleActionRequest request,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await salesServiceClient.ForwardCreateSaleAsync(
            request.CampaignId,
            new SalesCreateRequest(
                request.SoldWorldDay,
                request.StorageLocationId,
                request.CustomerId,
                request.Notes),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPut("sales/{saleId:guid}")]
    public async Task<IActionResult> UpdateSaleAsync(
        Guid saleId,
        [FromBody] UpdateSaleActionRequest request,
        CancellationToken cancellationToken)
    {
        if (saleId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("saleId is required."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await salesServiceClient.ForwardUpdateSaleAsync(
            request.CampaignId,
            saleId,
            new SalesUpdateRequest(
                request.SoldWorldDay,
                request.StorageLocationId,
                request.CustomerId,
                request.Notes,
                (request.Lines ?? []).Select(line => new SalesUpdateLineRequest(
                    line.SaleLineId,
                    line.ItemId,
                    line.Quantity,
                    line.UnitSoldPriceMinor,
                    line.UnitTrueValueMinor,
                    line.DiscountMinor,
                    line.Notes)).ToList(),
                (request.Payments ?? []).Select(payment => new SalesUpdatePaymentRequest(
                    payment.PaymentId,
                    payment.Method,
                    payment.AmountMinor,
                    payment.Details)).ToList()),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("sales/{saleId:guid}/complete")]
    public async Task<IActionResult> CompleteSaleAsync(
        Guid saleId,
        [FromBody] CompleteSaleActionRequest request,
        CancellationToken cancellationToken)
    {
        if (saleId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("saleId is required."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await salesServiceClient.ForwardCompleteSaleAsync(
            request.CampaignId,
            saleId,
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }

    [HttpPost("sales/{saleId:guid}/void")]
    public async Task<IActionResult> VoidSaleAsync(
        Guid saleId,
        [FromBody] VoidSaleActionRequest request,
        CancellationToken cancellationToken)
    {
        if (saleId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("saleId is required."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await salesServiceClient.ForwardVoidSaleAsync(
            request.CampaignId,
            saleId,
            new SalesVoidRequest(request.Reason),
            authorizationHeader,
            cancellationToken);

        return ToForwardedResult(response);
    }
}
