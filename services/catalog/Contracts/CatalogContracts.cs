namespace DndApp.Catalog.Contracts;

public enum ArchivedItemsFilter
{
    ActiveOnly,
    IncludeArchived,
    ArchivedOnly
}

public sealed record CategoryDto(Guid CategoryId, Guid CampaignId, string Name);

public sealed record CreateCategoryRequest(string Name);

public sealed record CreateCategoryResponse(Guid CategoryId);

public sealed record UnitDto(Guid UnitId, Guid CampaignId, string Name);

public sealed record CreateUnitRequest(string Name);

public sealed record CreateUnitResponse(Guid UnitId);

public sealed record TagDto(Guid TagId, Guid CampaignId, string Name);

public sealed record CreateTagRequest(string Name);

public sealed record CreateTagResponse(Guid TagId);

public sealed record ItemDto(
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

public sealed record CreateItemRequest(
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    long BaseValueMinor,
    long? DefaultListPriceMinor,
    decimal? Weight,
    Guid? ImageAssetId,
    IReadOnlyList<Guid>? TagIds);

public sealed record CreateItemResponse(Guid ItemId);

public sealed record UpdateItemRequest(
    string? Name,
    string? Description,
    Guid? CategoryId,
    Guid? UnitId,
    long? BaseValueMinor,
    long? DefaultListPriceMinor,
    decimal? Weight,
    Guid? ImageAssetId,
    IReadOnlyList<Guid>? TagIds);

public sealed record SetItemArchiveRequest(bool IsArchived);

public sealed record UpdateResultResponse(bool Updated);
