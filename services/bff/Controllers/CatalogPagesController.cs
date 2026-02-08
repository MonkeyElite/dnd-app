using DndApp.Bff.Clients;
using DndApp.Bff.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pages")]
public sealed class CatalogPagesController(
    CampaignServiceClient campaignServiceClient,
    CatalogServiceClient catalogServiceClient,
    IdentityServiceClient identityServiceClient) : CampaignAuthorizationControllerBase(identityServiceClient)
{
    [HttpGet("catalog")]
    public async Task<IActionResult> GetCatalogPageAsync(
        [FromQuery] Guid campaignId,
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? archived,
        CancellationToken cancellationToken)
    {
        var archivedValidationError = ValidateArchivedFilter(archived, out var normalizedArchived);
        if (archivedValidationError is not null)
        {
            return BadRequest(new ErrorResponse(archivedValidationError));
        }

        var permissionResult = await RequireCampaignRead(campaignId, cancellationToken);
        if (permissionResult is not null)
        {
            return permissionResult;
        }

        var authorizationHeader = Request.Headers.Authorization.ToString();

        var currencyTask = campaignServiceClient.ForwardGetCurrencySettingsAsync(campaignId, authorizationHeader, cancellationToken);
        var categoriesTask = catalogServiceClient.ForwardGetCategoriesAsync(campaignId, authorizationHeader, cancellationToken);
        var unitsTask = catalogServiceClient.ForwardGetUnitsAsync(campaignId, authorizationHeader, cancellationToken);
        var tagsTask = catalogServiceClient.ForwardGetTagsAsync(campaignId, authorizationHeader, cancellationToken);
        var itemsTask = catalogServiceClient.ForwardGetItemsAsync(
            campaignId,
            search,
            categoryId,
            normalizedArchived,
            authorizationHeader,
            cancellationToken);

        await Task.WhenAll(currencyTask, categoriesTask, unitsTask, tagsTask, itemsTask);

        if (!IsSuccessStatusCode(currencyTask.Result.StatusCode))
        {
            return ToForwardedResult(currencyTask.Result);
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

        var response = new CatalogPageResponse(
            campaignId,
            currency.CurrencyCode,
            new CatalogPageFiltersDto(
                categories
                    .Select(x => new CatalogPageFilterCategoryDto(x.CategoryId, x.Name))
                    .ToList(),
                units
                    .Select(x => new CatalogPageFilterUnitDto(x.UnitId, x.Name))
                    .ToList(),
                tags
                    .Select(x => new CatalogPageFilterTagDto(x.TagId, x.Name))
                    .ToList()),
            items
                .Select(item => new CatalogPageItemDto(
                    item.ItemId,
                    item.Name,
                    item.Description,
                    new CatalogPageItemCategoryDto(
                        item.CategoryId,
                        categoriesById.GetValueOrDefault(item.CategoryId, string.Empty)),
                    new CatalogPageItemUnitDto(
                        item.UnitId,
                        unitsById.GetValueOrDefault(item.UnitId, string.Empty)),
                    item.BaseValueMinor,
                    item.DefaultListPriceMinor,
                    item.Weight,
                    new CatalogPageItemImageDto(item.ImageAssetId, Url: null),
                    item.TagIds
                        .Select(tagId => new CatalogPageItemTagDto(
                            tagId,
                            tagsById.GetValueOrDefault(tagId, string.Empty)))
                        .ToList(),
                    item.IsArchived))
                .ToList());

        return Ok(response);
    }

    private static string? ValidateArchivedFilter(string? archived, out string? normalizedArchived)
    {
        if (string.IsNullOrWhiteSpace(archived))
        {
            normalizedArchived = null;
            return null;
        }

        if (Enum.TryParse<CatalogArchivedItemsFilter>(archived.Trim(), ignoreCase: true, out var parsed))
        {
            normalizedArchived = parsed.ToString();
            return null;
        }

        normalizedArchived = null;
        return "archived must be ActiveOnly, IncludeArchived, or ArchivedOnly.";
    }
}
