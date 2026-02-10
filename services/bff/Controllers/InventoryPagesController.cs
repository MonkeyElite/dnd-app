using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pages")]
public sealed class InventoryPagesController(
    CampaignServiceClient campaignServiceClient,
    CatalogServiceClient catalogServiceClient,
    InventoryServiceClient inventoryServiceClient,
    MediaServiceClient mediaServiceClient,
    IMemoryCache memoryCache,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    private const int DownloadUrlCacheMaxLifetimeSeconds = 5 * 60;

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
        var imageUrlsByAssetId = await ResolveImageUrlsByAssetIdAsync(
            campaignId,
            items,
            authorizationHeader,
            mediaServiceClient,
            cancellationToken);

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
                new InventoryPageImageDto(
                    item.ImageAssetId,
                    item.ImageAssetId.HasValue
                        ? imageUrlsByAssetId.GetValueOrDefault(item.ImageAssetId.Value)
                        : null),
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

    [HttpGet("campaign/{campaignId:guid}/inventory/summary")]
    public Task<IActionResult> GetCampaignInventorySummaryPageAsync(
        Guid campaignId,
        [FromQuery] Guid? placeId,
        [FromQuery] Guid? storageLocationId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => GetInventoryPageAsync(campaignId, placeId, storageLocationId, search, cancellationToken);

    [HttpGet("campaign/{campaignId:guid}/inventory/locations")]
    public async Task<IActionResult> GetCampaignInventoryLocationsPageAsync(
        Guid campaignId,
        CancellationToken cancellationToken)
    {
        var permissionResult = await RequireCampaignRead(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();
        var placesTask = campaignServiceClient.ForwardGetPlacesAsync(campaignId, authorizationHeader, cancellationToken);
        var storageLocationsTask = inventoryServiceClient.ForwardGetStorageLocationsAsync(
            campaignId,
            placeId: null,
            authorizationHeader,
            cancellationToken);
        var summaryTask = inventoryServiceClient.ForwardGetSummaryAsync(
            campaignId,
            placeId: null,
            storageLocationId: null,
            authorizationHeader,
            cancellationToken);

        await Task.WhenAll(placesTask, storageLocationsTask, summaryTask);

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

        var placeNamesById = places.ToDictionary(x => x.PlaceId, x => x.Name);
        var totalQuantityByLocation = summary.Rows
            .GroupBy(x => x.StorageLocationId)
            .ToDictionary(
                x => x.Key,
                x => x.Sum(y => y.OnHandQuantity));

        var locations = storageLocations
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(x => new InventoryLocationsPageRowDto(
                x.StorageLocationId,
                x.PlaceId,
                x.PlaceId.HasValue ? placeNamesById.GetValueOrDefault(x.PlaceId.Value) : null,
                x.Name,
                x.Type,
                x.Code,
                totalQuantityByLocation.GetValueOrDefault(x.StorageLocationId)))
            .ToList();

        return Ok(new InventoryLocationsPageResponse(campaignId, locations));
    }

    [HttpGet("campaign/{campaignId:guid}/inventory/location/{locationId:guid}")]
    public async Task<IActionResult> GetCampaignInventoryLocationDetailPageAsync(
        Guid campaignId,
        Guid locationId,
        CancellationToken cancellationToken)
    {
        if (locationId == Guid.Empty)
        {
            return BadRequest(new ErrorResponse("locationId is required."));
        }

        var permissionResult = await RequireCampaignRead(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();

        var currencyTask = campaignServiceClient.ForwardGetCurrencySettingsAsync(campaignId, authorizationHeader, cancellationToken);
        var placesTask = campaignServiceClient.ForwardGetPlacesAsync(campaignId, authorizationHeader, cancellationToken);
        var storageLocationsTask = inventoryServiceClient.ForwardGetStorageLocationsAsync(
            campaignId,
            placeId: null,
            authorizationHeader,
            cancellationToken);
        var lotsTask = inventoryServiceClient.ForwardGetLotsAsync(
            campaignId,
            itemId: null,
            storageLocationId: locationId,
            authorizationHeader,
            cancellationToken);
        var adjustmentsTask = inventoryServiceClient.ForwardGetAdjustmentsAsync(
            campaignId,
            fromWorldDay: null,
            toWorldDay: null,
            itemId: null,
            storageLocationId: locationId,
            authorizationHeader,
            cancellationToken);
        var itemsTask = catalogServiceClient.ForwardGetItemsAsync(
            campaignId,
            search: null,
            categoryId: null,
            archived: CatalogArchivedItemsFilter.IncludeArchived.ToString(),
            authorizationHeader,
            cancellationToken);

        await Task.WhenAll(currencyTask, placesTask, storageLocationsTask, lotsTask, adjustmentsTask, itemsTask);

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

        if (!IsSuccessStatusCode(lotsTask.Result.StatusCode))
        {
            return ToForwardedResult(lotsTask.Result);
        }

        if (!IsSuccessStatusCode(adjustmentsTask.Result.StatusCode))
        {
            return ToForwardedResult(adjustmentsTask.Result);
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

        var lots = DeserializeBody<List<InventoryLotDto>>(lotsTask.Result.Body);
        if (lots is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Inventory service returned invalid lots JSON."));
        }

        var adjustments = DeserializeBody<List<InventoryAdjustmentDto>>(adjustmentsTask.Result.Body);
        if (adjustments is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Inventory service returned invalid adjustments JSON."));
        }

        var items = DeserializeBody<List<CatalogItemDto>>(itemsTask.Result.Body);
        if (items is null)
        {
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new ErrorResponse("Catalog service returned invalid items JSON."));
        }

        var location = storageLocations.SingleOrDefault(x => x.StorageLocationId == locationId);
        if (location is null)
        {
            return NotFound(new ErrorResponse("Storage location not found."));
        }

        var placeNamesById = places.ToDictionary(x => x.PlaceId, x => x.Name);
        var itemNamesById = items.ToDictionary(x => x.ItemId, x => x.Name);

        var lotRows = lots
            .Select(x => new InventoryLocationDetailLotPageRowDto(
                x.LotId,
                x.ItemId,
                itemNamesById.GetValueOrDefault(x.ItemId, string.Empty),
                x.QuantityOnHand,
                x.UnitCostMinor,
                x.AcquiredWorldDay,
                x.Source,
                x.Notes))
            .ToList();

        var adjustmentRows = adjustments
            .Select(x => new InventoryLocationDetailAdjustmentPageRowDto(
                x.AdjustmentId,
                x.ItemId,
                itemNamesById.GetValueOrDefault(x.ItemId, string.Empty),
                x.LotId,
                x.DeltaQuantity,
                x.Reason,
                x.WorldDay,
                x.Notes,
                x.ReferenceType,
                x.ReferenceId,
                x.CreatedAt))
            .ToList();

        return Ok(new InventoryLocationDetailPageResponse(
            campaignId,
            location.StorageLocationId,
            location.Name,
            location.Code,
            location.Type,
            location.PlaceId,
            location.PlaceId.HasValue ? placeNamesById.GetValueOrDefault(location.PlaceId.Value) : null,
            currency.CurrencyCode,
            lotRows,
            adjustmentRows));
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

    private async Task<Dictionary<Guid, string?>> ResolveImageUrlsByAssetIdAsync(
        Guid campaignId,
        IReadOnlyCollection<CatalogItemDto> items,
        string authorizationHeader,
        MediaServiceClient mediaServiceClient,
        CancellationToken cancellationToken)
    {
        var assetIds = items
            .Where(x => x.ImageAssetId.HasValue)
            .Select(x => x.ImageAssetId!.Value)
            .Distinct()
            .ToList();

        if (assetIds.Count == 0)
        {
            return [];
        }

        var tasks = assetIds.ToDictionary(
            assetId => assetId,
            assetId => TryGetDownloadUrlAsync(
                campaignId,
                assetId,
                authorizationHeader,
                mediaServiceClient,
                memoryCache,
                cancellationToken));

        await Task.WhenAll(tasks.Values);

        return tasks.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Result);
    }

    private async Task<string?> TryGetDownloadUrlAsync(
        Guid campaignId,
        Guid assetId,
        string authorizationHeader,
        MediaServiceClient mediaServiceClient,
        IMemoryCache memoryCache,
        CancellationToken cancellationToken)
    {
        var cacheKey = CreateDownloadUrlCacheKey(campaignId, assetId);
        if (memoryCache.TryGetValue(cacheKey, out CachedDownloadUrl? cached)
            && cached is not null
            && !string.IsNullOrWhiteSpace(cached.Url)
            && cached.ExpiresAt > DateTimeOffset.UtcNow.AddSeconds(30))
        {
            return cached.Url;
        }

        var response = await mediaServiceClient.ForwardGetDownloadUrlAsync(
            campaignId,
            assetId,
            expiresInSeconds: null,
            authorizationHeader,
            cancellationToken);

        if (!IsSuccessStatusCode(response.StatusCode))
        {
            return null;
        }

        var payload = DeserializeBody<MediaDownloadUrlResponse>(response.Body);
        if (payload is null || string.IsNullOrWhiteSpace(payload.Url))
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var cacheDuration = payload.ExpiresAt - now;
        if (cacheDuration > TimeSpan.Zero)
        {
            var maxDuration = TimeSpan.FromSeconds(DownloadUrlCacheMaxLifetimeSeconds);
            var effectiveDuration = cacheDuration > maxDuration ? maxDuration : cacheDuration;
            memoryCache.Set(cacheKey, new CachedDownloadUrl(payload.Url, payload.ExpiresAt), effectiveDuration);
        }

        return payload.Url;
    }

    private static string CreateDownloadUrlCacheKey(Guid campaignId, Guid assetId)
        => $"media-download-url:{campaignId:D}:{assetId:D}";

    private sealed record CachedDownloadUrl(string Url, DateTimeOffset ExpiresAt);
}
