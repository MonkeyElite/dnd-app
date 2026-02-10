using System.Text.Json;
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
    CatalogServiceClient catalogServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

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

    [HttpPost("sales/draft/create")]
    public Task<IActionResult> CreateDraftSaleAsync(
        [FromBody] SalesDraftCreateActionRequest request,
        CancellationToken cancellationToken)
        => CreateSaleAsync(
            new CreateSaleActionRequest(
                request.CampaignId,
                request.SoldWorldDay,
                request.StorageLocationId,
                request.CustomerId,
                request.Notes),
            cancellationToken);

    [HttpPost("sales/draft/add-line")]
    public async Task<IActionResult> AddDraftSaleLineAsync(
        [FromBody] SalesDraftAddLineActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DraftId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("draftId is required."));
        }

        if (request.ItemId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("itemId is required."));
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(new ErrorResponse("quantity must be greater than 0."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var draftLookup = await TryGetDraftSaleAsync(
            request.CampaignId,
            request.DraftId,
            authorizationHeader,
            cancellationToken);

        if (draftLookup.ErrorResult is not null)
        {
            return draftLookup.ErrorResult;
        }

        var draftSale = draftLookup.Draft!;
        var unitSoldPriceMinor = request.UnitSoldPriceMinor ?? await ResolveDefaultUnitPriceAsync(
            request.CampaignId,
            request.ItemId,
            authorizationHeader,
            cancellationToken);

        if (unitSoldPriceMinor is null)
        {
            return BadRequest(new ErrorResponse("Unable to resolve default price for item."));
        }

        var lines = draftSale.Lines
            .Select(ToUpdateSaleLine)
            .ToList();

        lines.Add(new SalesUpdateLineRequest(
            SaleLineId: null,
            ItemId: request.ItemId,
            Quantity: request.Quantity,
            UnitSoldPriceMinor: unitSoldPriceMinor.Value,
            UnitTrueValueMinor: request.UnitTrueValueMinor ?? unitSoldPriceMinor.Value,
            DiscountMinor: request.DiscountMinor ?? 0,
            Notes: request.Notes));

        var updateRequest = BuildDraftUpdateRequest(draftSale, lines);
        var updateResponse = await salesServiceClient.ForwardUpdateSaleAsync(
            request.CampaignId,
            request.DraftId,
            updateRequest,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(updateResponse.StatusCode))
        {
            return ToForwardedResult(updateResponse);
        }

        return Ok(new SalesDraftMutationActionResponse(request.DraftId, Updated: true));
    }

    [HttpPost("sales/draft/update-line")]
    public async Task<IActionResult> UpdateDraftSaleLineAsync(
        [FromBody] SalesDraftUpdateLineActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DraftId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("draftId is required."));
        }

        if (request.SaleLineId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("saleLineId is required."));
        }

        if (request.Quantity <= 0)
        {
            return BadRequest(new ErrorResponse("quantity must be greater than 0."));
        }

        if (request.UnitSoldPriceMinor < 0)
        {
            return BadRequest(new ErrorResponse("unitSoldPriceMinor must be greater than or equal to 0."));
        }

        if (request.DiscountMinor < 0)
        {
            return BadRequest(new ErrorResponse("discountMinor must be greater than or equal to 0."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var draftLookup = await TryGetDraftSaleAsync(
            request.CampaignId,
            request.DraftId,
            authorizationHeader,
            cancellationToken);

        if (draftLookup.ErrorResult is not null)
        {
            return draftLookup.ErrorResult;
        }

        var draftSale = draftLookup.Draft!;
        var lineUpdated = false;

        var lines = draftSale.Lines
            .Select(line =>
            {
                if (line.SaleLineId != request.SaleLineId)
                {
                    return ToUpdateSaleLine(line);
                }

                lineUpdated = true;
                return new SalesUpdateLineRequest(
                    request.SaleLineId,
                    line.ItemId,
                    request.Quantity,
                    request.UnitSoldPriceMinor,
                    request.UnitTrueValueMinor,
                    request.DiscountMinor,
                    request.Notes);
            })
            .ToList();

        if (!lineUpdated)
        {
            return NotFound(new ErrorResponse("Sale line not found in draft."));
        }

        var updateRequest = BuildDraftUpdateRequest(draftSale, lines);
        var updateResponse = await salesServiceClient.ForwardUpdateSaleAsync(
            request.CampaignId,
            request.DraftId,
            updateRequest,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(updateResponse.StatusCode))
        {
            return ToForwardedResult(updateResponse);
        }

        return Ok(new SalesDraftMutationActionResponse(request.DraftId, Updated: true));
    }

    [HttpPost("sales/draft/remove-line")]
    public async Task<IActionResult> RemoveDraftSaleLineAsync(
        [FromBody] SalesDraftRemoveLineActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DraftId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("draftId is required."));
        }

        if (request.SaleLineId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("saleLineId is required."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var draftLookup = await TryGetDraftSaleAsync(
            request.CampaignId,
            request.DraftId,
            authorizationHeader,
            cancellationToken);

        if (draftLookup.ErrorResult is not null)
        {
            return draftLookup.ErrorResult;
        }

        var draftSale = draftLookup.Draft!;
        var lines = draftSale.Lines
            .Where(x => x.SaleLineId != request.SaleLineId)
            .Select(ToUpdateSaleLine)
            .ToList();

        if (lines.Count == draftSale.Lines.Count)
        {
            return NotFound(new ErrorResponse("Sale line not found in draft."));
        }

        var updateRequest = BuildDraftUpdateRequest(draftSale, lines);
        var updateResponse = await salesServiceClient.ForwardUpdateSaleAsync(
            request.CampaignId,
            request.DraftId,
            updateRequest,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(updateResponse.StatusCode))
        {
            return ToForwardedResult(updateResponse);
        }

        return Ok(new SalesDraftMutationActionResponse(request.DraftId, Updated: true));
    }

    [HttpPost("sales/draft/complete")]
    public async Task<IActionResult> CompleteDraftSaleAsync(
        [FromBody] SalesDraftCompleteActionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.DraftId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("draftId is required."));
        }

        var permissionResult = await RequireCampaignWrite(request.CampaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var response = await salesServiceClient.ForwardCompleteSaleAsync(
            request.CampaignId,
            request.DraftId,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(response.StatusCode))
        {
            return ToForwardedResult(response);
        }

        var payload = DeserializeBodyOrDefault<SalesCompleteResponse>(response.Body);
        var status = payload?.Status ?? "Completed";
        return Ok(new SalesDraftCompleteActionResponse(request.DraftId, status));
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

    private async Task<long?> ResolveDefaultUnitPriceAsync(
        Guid campaignId,
        Guid itemId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var itemsResponse = await catalogServiceClient.ForwardGetItemsAsync(
            campaignId,
            search: null,
            categoryId: null,
            archived: CatalogArchivedItemsFilter.IncludeArchived.ToString(),
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(itemsResponse.StatusCode))
        {
            return null;
        }

        var items = DeserializeBodyOrDefault<List<CatalogItemDto>>(itemsResponse.Body);
        var item = items?.FirstOrDefault(x => x.ItemId == itemId);
        if (item is null)
        {
            return null;
        }

        return item.DefaultListPriceMinor ?? item.BaseValueMinor;
    }

    private async Task<(SalesDetailDto? Draft, IActionResult? ErrorResult)> TryGetDraftSaleAsync(
        Guid campaignId,
        Guid draftId,
        string authorizationHeader,
        CancellationToken cancellationToken)
    {
        var response = await salesServiceClient.ForwardGetSaleAsync(
            campaignId,
            draftId,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(response.StatusCode))
        {
            return (null, ToForwardedResult(response));
        }

        var draft = DeserializeBodyOrDefault<SalesDetailDto>(response.Body);
        if (draft is null)
        {
            return (
                null,
                StatusCode(
                    StatusCodes.Status502BadGateway,
                    new ErrorResponse("Sales service returned invalid sale JSON.")));
        }

        if (!draft.Status.Equals("Draft", StringComparison.OrdinalIgnoreCase))
        {
            return (null, Conflict(new ErrorResponse("Only draft sales can be modified.")));
        }

        return (draft, null);
    }

    private static SalesUpdateRequest BuildDraftUpdateRequest(
        SalesDetailDto draft,
        IReadOnlyList<SalesUpdateLineRequest> lines)
        => new(
            draft.SoldWorldDay,
            draft.StorageLocationId,
            draft.CustomerId,
            draft.Notes,
            lines,
            draft.Payments.Select(ToUpdateSalePayment).ToList());

    private static SalesUpdateLineRequest ToUpdateSaleLine(SalesLineDto line)
        => new(
            line.SaleLineId,
            line.ItemId,
            line.Quantity,
            line.UnitSoldPriceMinor,
            line.UnitTrueValueMinor,
            line.DiscountMinor,
            line.Notes);

    private static SalesUpdatePaymentRequest ToUpdateSalePayment(SalesPaymentDto payment)
        => new(payment.PaymentId, payment.Method, payment.AmountMinor, payment.Details);

    private static T? DeserializeBodyOrDefault<T>(string body)
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
}
