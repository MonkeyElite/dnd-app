using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pages")]
public sealed class SalesPagesController(
    CampaignServiceClient campaignServiceClient,
    SalesServiceClient salesServiceClient,
    InventoryServiceClient inventoryServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpGet("sales")]
    public async Task<IActionResult> GetSalesPageAsync(
        [FromQuery] Guid campaignId,
        [FromQuery] int? fromWorldDay,
        [FromQuery] int? toWorldDay,
        [FromQuery] Guid? customerId,
        CancellationToken cancellationToken)
    {
        if (customerId.HasValue && customerId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("customerId must be a non-empty GUID when provided."));
        }

        var permissionResult = await RequireCampaignRead(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();

        var currencyTask = campaignServiceClient.ForwardGetCurrencySettingsAsync(campaignId, authorizationHeader, cancellationToken);
        var customersTask = campaignServiceClient.ForwardGetCustomersAsync(campaignId, null, authorizationHeader, cancellationToken);
        var salesTask = salesServiceClient.ForwardGetSalesAsync(
            campaignId,
            fromWorldDay,
            toWorldDay,
            customerId,
            authorizationHeader,
            cancellationToken);

        await Task.WhenAll(currencyTask, customersTask, salesTask);

        if (!IsSuccessStatusCode(currencyTask.Result.StatusCode))
        {
            return ToForwardedResult(currencyTask.Result);
        }

        if (!IsSuccessStatusCode(customersTask.Result.StatusCode))
        {
            return ToForwardedResult(customersTask.Result);
        }

        if (!IsSuccessStatusCode(salesTask.Result.StatusCode))
        {
            return ToForwardedResult(salesTask.Result);
        }

        var currency = DeserializeBody<CurrencyConfigDto>(currencyTask.Result.Body);
        if (currency is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid currency JSON."));
        }

        var customers = DeserializeBody<List<CampaignCustomerDto>>(customersTask.Result.Body);
        if (customers is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid customers JSON."));
        }

        var sales = DeserializeBody<List<SalesListItemDto>>(salesTask.Result.Body);
        if (sales is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Sales service returned invalid sales JSON."));
        }

        var customersById = customers.ToDictionary(x => x.CustomerId, x => x.Name);
        var rows = sales
            .Select(x => new SalesPageRowDto(
                x.SaleId,
                x.Status,
                x.SoldWorldDay,
                x.CustomerId.HasValue ? customersById.GetValueOrDefault(x.CustomerId.Value) : null,
                x.TotalMinor))
            .ToList();

        var filters = new SalesPageFiltersDto(
            customers
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SalesPageFilterCustomerDto(x.CustomerId, x.Name))
                .ToList());

        return Ok(new SalesPageResponse(campaignId, currency.CurrencyCode, filters, rows));
    }

    [HttpGet("sale")]
    public async Task<IActionResult> GetSalePageAsync(
        [FromQuery] Guid campaignId,
        [FromQuery] Guid saleId,
        CancellationToken cancellationToken)
    {
        if (saleId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("saleId is required."));
        }

        var permissionResult = await RequireCampaignRead(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var currencyTask = campaignServiceClient.ForwardGetCurrencySettingsAsync(campaignId, authorizationHeader, cancellationToken);
        var customersTask = campaignServiceClient.ForwardGetCustomersAsync(campaignId, null, authorizationHeader, cancellationToken);
        var storageLocationsTask = inventoryServiceClient.ForwardGetStorageLocationsAsync(
            campaignId,
            null,
            authorizationHeader,
            cancellationToken);
        var saleTask = salesServiceClient.ForwardGetSaleAsync(campaignId, saleId, authorizationHeader, cancellationToken);

        await Task.WhenAll(currencyTask, customersTask, storageLocationsTask, saleTask);

        if (!IsSuccessStatusCode(currencyTask.Result.StatusCode))
        {
            return ToForwardedResult(currencyTask.Result);
        }

        if (!IsSuccessStatusCode(customersTask.Result.StatusCode))
        {
            return ToForwardedResult(customersTask.Result);
        }

        if (!IsSuccessStatusCode(storageLocationsTask.Result.StatusCode))
        {
            return ToForwardedResult(storageLocationsTask.Result);
        }

        if (!IsSuccessStatusCode(saleTask.Result.StatusCode))
        {
            return ToForwardedResult(saleTask.Result);
        }

        var currency = DeserializeBody<CurrencyConfigDto>(currencyTask.Result.Body);
        if (currency is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid currency JSON."));
        }

        var customers = DeserializeBody<List<CampaignCustomerDto>>(customersTask.Result.Body);
        if (customers is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid customers JSON."));
        }

        var storageLocations = DeserializeBody<List<InventoryStorageLocationDto>>(storageLocationsTask.Result.Body);
        if (storageLocations is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Inventory service returned invalid storage locations JSON."));
        }

        var sale = DeserializeBody<SalesDetailDto>(saleTask.Result.Body);
        if (sale is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Sales service returned invalid sale JSON."));
        }

        var filters = new SalePageFiltersDto(
            customers
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SalesPageFilterCustomerDto(x.CustomerId, x.Name))
                .ToList(),
            storageLocations
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new SalePageFilterStorageLocationDto(x.StorageLocationId, x.Name))
                .ToList());

        return Ok(new SalePageResponse(campaignId, currency.CurrencyCode, sale, filters));
    }
}
