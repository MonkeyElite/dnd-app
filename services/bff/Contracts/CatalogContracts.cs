namespace DndApp.Bff.Contracts;

public enum CatalogArchivedItemsFilter
{
    ActiveOnly,
    IncludeArchived,
    ArchivedOnly
}

public sealed record CatalogCategoryDto(Guid CategoryId, Guid CampaignId, string Name);

public sealed record CatalogCreateCategoryRequest(string Name);

public sealed record CatalogCreateCategoryResponse(Guid CategoryId);

public sealed record CatalogUnitDto(Guid UnitId, Guid CampaignId, string Name);

public sealed record CatalogCreateUnitRequest(string Name);

public sealed record CatalogCreateUnitResponse(Guid UnitId);

public sealed record CatalogTagDto(Guid TagId, Guid CampaignId, string Name);

public sealed record CatalogCreateTagRequest(string Name);

public sealed record CatalogCreateTagResponse(Guid TagId);

public sealed record CatalogItemDto(
    Guid ItemId,
    Guid CampaignId,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    long BaseValueMinor,
    long? DefaultListPriceMinor,
    decimal? Weight,
    Guid? ImageAssetId,
    IReadOnlyList<Guid> TagIds,
    bool IsArchived);

public sealed record CatalogCreateItemRequest(
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    long BaseValueMinor,
    long? DefaultListPriceMinor,
    decimal? Weight,
    Guid? ImageAssetId,
    IReadOnlyList<Guid>? TagIds);

public sealed record CatalogCreateItemResponse(Guid ItemId);

public sealed record CatalogSetItemArchiveRequest(bool IsArchived);

public sealed record CatalogUpdateResultResponse(bool Updated);

public sealed record CreateCategoryActionRequest(Guid CampaignId, string Name);

public sealed record CreateUnitActionRequest(Guid CampaignId, string Name);

public sealed record CreateTagActionRequest(Guid CampaignId, string Name);

public sealed record CreateItemActionRequest(
    Guid CampaignId,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    long BaseValueMinor,
    long? DefaultListPriceMinor,
    decimal? Weight,
    Guid? ImageAssetId,
    IReadOnlyList<Guid>? TagIds);

public sealed record SetItemArchiveActionRequest(Guid CampaignId, bool IsArchived);

public sealed record CatalogPageFiltersDto(
    IReadOnlyList<CatalogPageFilterCategoryDto> Categories,
    IReadOnlyList<CatalogPageFilterUnitDto> Units,
    IReadOnlyList<CatalogPageFilterTagDto> Tags);

public sealed record CatalogPageFilterCategoryDto(Guid CategoryId, string Name);

public sealed record CatalogPageFilterUnitDto(Guid UnitId, string Name);

public sealed record CatalogPageFilterTagDto(Guid TagId, string Name);

public sealed record CatalogPageItemCategoryDto(Guid CategoryId, string Name);

public sealed record CatalogPageItemUnitDto(Guid UnitId, string Name);

public sealed record CatalogPageItemImageDto(Guid? AssetId, string? Url);

public sealed record CatalogPageItemTagDto(Guid TagId, string Name);

public sealed record CatalogPageItemDto(
    Guid ItemId,
    string Name,
    string? Description,
    CatalogPageItemCategoryDto Category,
    CatalogPageItemUnitDto Unit,
    long BaseValueMinor,
    long? DefaultListPriceMinor,
    decimal? Weight,
    CatalogPageItemImageDto Image,
    IReadOnlyList<CatalogPageItemTagDto> Tags,
    bool IsArchived);

public sealed record CatalogPageResponse(
    Guid CampaignId,
    string CurrencyCode,
    CatalogPageFiltersDto Filters,
    IReadOnlyList<CatalogPageItemDto> Items);

public sealed record CatalogItemPageResponse(
    Guid CampaignId,
    string CurrencyCode,
    CatalogPageItemDto Item);
