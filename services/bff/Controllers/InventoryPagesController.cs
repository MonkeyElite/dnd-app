using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pages")]
public sealed class InventoryPagesController(
    CampaignServiceClient campaignServiceClient,
    CatalogServiceClient catalogServiceClient,
    InventoryServiceClient inventoryServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventoryPageAsync(
        [FromQuery] Guid campaignId,
        [FromQuery] Guid? placeId,
        [FromQuery] Guid? storageLocationId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        if (placeId.HasValue && placeId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("placeId must be a non-empty GUID when provided."));
        }

        if (storageLocationId.HasValue && storageLocationId.Value == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("storageLocationId must be a non-empty GUID when provided."));
        }

        var permissionResult = await RequireCampaignRead(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();

        var currencyTask = campaignServiceClient.ForwardGetCurrencySettingsAsync(campaignId, authorizationHeader, cancellationToken);
        var placesTask = campaignServiceClient.ForwardGetPlacesAsync(campaignId, authorizationHeader, cancellationToken);
        var storageLocationsTask = inventoryServiceClient.ForwardGetStorageLocationsAsync(campaignId, placeId, authorizationHeader, cancellationToken);
        var summaryTask = inventoryServiceClient.ForwardGetSummaryAsync(campaignId, placeId, storageLocationId, authorizationHeader, cancellationToken);
        var categoriesTask = catalogServiceClient.ForwardGetCategoriesAsync(campaignId, authorizationHeader, cancellationToken);
        var unitsTask = catalogServiceClient.ForwardGetUnitsAsync(campaignId, authorizationHeader, cancellationToken);
        var tagsTask = catalogServiceClient.ForwardGetTagsAsync(campaignId, authorizationHeader, cancellationToken);
        var itemsTask = catalogServiceClient.ForwardGetItemsAsync(
            campaignId,
            search: null,
            categoryId: null,
            archived: CatalogArchivedItemsFilter.IncludeArchived.ToString(),
            authorizationHeader,
            cancellationToken);

        await Task.WhenAll(
            currencyTask,
            placesTask,
            storageLocationsTask,
            summaryTask,
            categoriesTask,
            unitsTask,
            tagsTask,
            itemsTask);

        if (!IsSuccessStatusCode(currencyTask.Result.StatusCode))
        {
            return ToForwardedResult(currencyTask.Result);
        }

        if (!IsSuccessStatusCode(placesTask.Result.StatusCode))
        {
            return ToForwardedResult(placesTask.Result);
        }

        if (!IsSuccessStatusCode(storageLocationsTask.Result.StatusCode))
        {
            return ToForwardedResult(storageLocationsTask.Result);
        }

        if (!IsSuccessStatusCode(summaryTask.Result.StatusCode))
        {
            return ToForwardedResult(summaryTask.Result);
        }

        if (!IsSuccessStatusCode(categoriesTask.Result.StatusCode))
        {
            return ToForwardedResult(categoriesTask.Result);
        }

        if (!IsSuccessStatusCode(unitsTask.Result.StatusCode))
        {
            return ToForwardedResult(unitsTask.Result);
        }

        if (!IsSuccessStatusCode(tagsTask.Result.StatusCode))
        {
            return ToForwardedResult(tagsTask.Result);
        }

        if (!IsSuccessStatusCode(itemsTask.Result.StatusCode))
        {
            return ToForwardedResult(itemsTask.Result);
        }

        var currency = DeserializeBody<CurrencyConfigDto>(currencyTask.Result.Body);
        if (currency is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid currency JSON."));
        }

        var places = DeserializeBody<List<CampaignPlaceDto>>(placesTask.Result.Body);
        if (places is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Campaign service returned invalid places JSON."));
        }

        var storageLocations = DeserializeBody<List<InventoryStorageLocationDto>>(storageLocationsTask.Result.Body);
        if (storageLocations is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Inventory service returned invalid storage locations JSON."));
        }

        var summary = DeserializeBody<InventorySummaryResponse>(summaryTask.Result.Body);
        if (summary is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Inventory service returned invalid summary JSON."));
        }

        var categories = DeserializeBody<List<CatalogCategoryDto>>(categoriesTask.Result.Body);
        if (categories is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Catalog service returned invalid categories JSON."));
        }

        var units = DeserializeBody<List<CatalogUnitDto>>(unitsTask.Result.Body);
        if (units is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Catalog service returned invalid units JSON."));
        }

        var tags = DeserializeBody<List<CatalogTagDto>>(tagsTask.Result.Body);
        if (tags is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Catalog service returned invalid tags JSON."));
        }

        var items = DeserializeBody<List<CatalogItemDto>>(itemsTask.Result.Body);
        if (items is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Catalog service returned invalid items JSON."));
        }

        var categoriesById = categories.ToDictionary(x => x.CategoryId, x => x.Name);
        var unitsById = units.ToDictionary(x => x.UnitId, x => x.Name);
        var tagsById = tags.ToDictionary(x => x.TagId, x => x.Name);
        var itemsById = items.ToDictionary(x => x.ItemId);
        var storageLocationsById = storageLocations.ToDictionary(x => x.StorageLocationId);

        var rows = new List<InventoryPageRowDto>();
        foreach (var summaryRow in summary.Rows)
        {
            if (!itemsById.TryGetValue(summaryRow.ItemId, out var item))
            {
                continue;
            }

            if (!storageLocationsById.TryGetValue(summaryRow.StorageLocationId, out var location))
            {
                continue;
            }

            var categoryName = categoriesById.GetValueOrDefault(item.CategoryId, string.Empty);
            var unitName = unitsById.GetValueOrDefault(item.UnitId, string.Empty);

            var tagNames = item.TagIds
                .Select(tagId => tagsById.GetValueOrDefault(tagId, string.Empty))
                .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
                .ToList();

            if (!MatchesSearch(search, item.Name, categoryName, tagNames))
            {
                continue;
            }

            rows.Add(new InventoryPageRowDto(
                summaryRow.ItemId,
                item.Name,
                categoryName,
                unitName,
                new InventoryPageImageDto(item.ImageAssetId, Url: null),
                summaryRow.StorageLocationId,
                location.Name,
                summaryRow.OnHandQuantity));
        }

        var sortedRows = rows
            .OrderBy(x => x.ItemName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.StorageLocationName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var filters = new InventoryPageFiltersDto(
            places
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new InventoryPageFilterPlaceDto(x.PlaceId, x.Name, x.Type))
                .ToList(),
            storageLocations
                .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new InventoryPageFilterStorageLocationDto(
                    x.StorageLocationId,
                    x.PlaceId,
                    x.Name,
                    x.Type,
                    x.Code))
                .ToList());

        return Ok(new InventoryPageResponse(campaignId, currency.CurrencyCode, filters, sortedRows));
    }

    private static bool MatchesSearch(
        string? search,
        string itemName,
        string categoryName,
        IReadOnlyCollection<string> tagNames)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        var normalizedSearch = search.Trim();
        if (itemName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (categoryName.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return tagNames.Any(x => x.Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
    }
}
