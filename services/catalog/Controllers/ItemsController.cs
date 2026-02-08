using System.Text.Json;
using DndApp.Catalog.Contracts;
using DndApp.Catalog.Data;
using DndApp.Catalog.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Catalog.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/items")]
public sealed class ItemsController(CatalogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid campaignId,
        [FromQuery] string? search,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? archived,
        CancellationToken cancellationToken)
    {
        var archivedFilterError = TryParseArchivedFilter(archived, out var archivedFilter);
        if (archivedFilterError is not null)
        {
            return BadRequest(new ErrorResponse(archivedFilterError));
        }

        var query = dbContext.Items
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search.Trim()}%"));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        query = archivedFilter switch
        {
            ArchivedItemsFilter.ActiveOnly => query.Where(x => !x.IsArchived),
            ArchivedItemsFilter.ArchivedOnly => query.Where(x => x.IsArchived),
            _ => query
        };

        var items = await query
            .OrderBy(x => x.Name)
            .Select(x => new ItemDto(
                x.ItemId,
                x.CampaignId,
                x.Name,
                x.Description,
                x.CategoryId,
                x.UnitId,
                x.BaseValueMinor,
                x.DefaultListPriceMinor,
                x.Weight,
                x.ImageAssetId,
                x.ItemTags.OrderBy(y => y.TagId).Select(y => y.TagId).ToList(),
                x.IsArchived))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateItemRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateRequest(request, out var normalizedRequest);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        if (!await CategoryExistsAsync(campaignId, normalizedRequest.CategoryId, cancellationToken))
        {
            return BadRequest(new ErrorResponse("categoryId is invalid for this campaign."));
        }

        if (!await UnitExistsAsync(campaignId, normalizedRequest.UnitId, cancellationToken))
        {
            return BadRequest(new ErrorResponse("unitId is invalid for this campaign."));
        }

        if (!await AllTagsExistAsync(campaignId, normalizedRequest.TagIds, cancellationToken))
        {
            return BadRequest(new ErrorResponse("tagIds contains unknown tag ids for this campaign."));
        }

        var now = DateTimeOffset.UtcNow;
        var item = new Item
        {
            ItemId = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = normalizedRequest.Name,
            Description = normalizedRequest.Description,
            CategoryId = normalizedRequest.CategoryId,
            UnitId = normalizedRequest.UnitId,
            BaseValueMinor = normalizedRequest.BaseValueMinor,
            DefaultListPriceMinor = normalizedRequest.DefaultListPriceMinor,
            Weight = normalizedRequest.Weight,
            ImageAssetId = normalizedRequest.ImageAssetId,
            IsArchived = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        foreach (var tagId in normalizedRequest.TagIds)
        {
            item.ItemTags.Add(new ItemTag
            {
                ItemId = item.ItemId,
                TagId = tagId
            });
        }

        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateItemResponse(item.ItemId));
    }

    [HttpPut("{itemId:guid}")]
    public async Task<IActionResult> UpdateAsync(
        Guid campaignId,
        Guid itemId,
        [FromBody] JsonElement requestBody,
        CancellationToken cancellationToken)
    {
        var validationError = TryParseUpdateRequest(requestBody, out var patch);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        if (!patch.HasAnyChanges)
        {
            return BadRequest(new ErrorResponse("At least one field must be provided for update."));
        }

        var item = await dbContext.Items
            .Include(x => x.ItemTags)
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.ItemId == itemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new ErrorResponse("Item not found."));
        }

        if (patch.HasCategoryId && !await CategoryExistsAsync(campaignId, patch.CategoryId!.Value, cancellationToken))
        {
            return BadRequest(new ErrorResponse("categoryId is invalid for this campaign."));
        }

        if (patch.HasUnitId && !await UnitExistsAsync(campaignId, patch.UnitId!.Value, cancellationToken))
        {
            return BadRequest(new ErrorResponse("unitId is invalid for this campaign."));
        }

        if (patch.HasTagIds && !await AllTagsExistAsync(campaignId, patch.TagIds!, cancellationToken))
        {
            return BadRequest(new ErrorResponse("tagIds contains unknown tag ids for this campaign."));
        }

        if (patch.HasName)
        {
            item.Name = patch.Name!;
        }

        if (patch.HasDescription)
        {
            item.Description = patch.Description;
        }

        if (patch.HasCategoryId)
        {
            item.CategoryId = patch.CategoryId!.Value;
        }

        if (patch.HasUnitId)
        {
            item.UnitId = patch.UnitId!.Value;
        }

        if (patch.HasBaseValueMinor)
        {
            item.BaseValueMinor = patch.BaseValueMinor!.Value;
        }

        if (patch.HasDefaultListPriceMinor)
        {
            item.DefaultListPriceMinor = patch.DefaultListPriceMinor;
        }

        if (patch.HasWeight)
        {
            item.Weight = patch.Weight;
        }

        if (patch.HasImageAssetId)
        {
            item.ImageAssetId = patch.ImageAssetId;
        }

        if (patch.HasTagIds)
        {
            dbContext.ItemTags.RemoveRange(item.ItemTags);
            item.ItemTags.Clear();

            foreach (var tagId in patch.TagIds!)
            {
                item.ItemTags.Add(new ItemTag
                {
                    ItemId = item.ItemId,
                    TagId = tagId
                });
            }
        }

        item.UpdatedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new UpdateResultResponse(true));
    }

    [HttpPost("{itemId:guid}/archive")]
    public async Task<IActionResult> SetArchiveStateAsync(
        Guid campaignId,
        Guid itemId,
        [FromBody] SetItemArchiveRequest request,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.Items
            .SingleOrDefaultAsync(x => x.CampaignId == campaignId && x.ItemId == itemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new ErrorResponse("Item not found."));
        }

        item.IsArchived = request.IsArchived;
        item.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new UpdateResultResponse(true));
    }

    private async Task<bool> CategoryExistsAsync(Guid campaignId, Guid categoryId, CancellationToken cancellationToken)
        => await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId == campaignId && x.CategoryId == categoryId, cancellationToken);

    private async Task<bool> UnitExistsAsync(Guid campaignId, Guid unitId, CancellationToken cancellationToken)
        => await dbContext.Units
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId == campaignId && x.UnitId == unitId, cancellationToken);

    private async Task<bool> AllTagsExistAsync(
        Guid campaignId,
        IReadOnlyCollection<Guid> tagIds,
        CancellationToken cancellationToken)
    {
        if (tagIds.Count == 0)
        {
            return true;
        }

        var distinctTagIds = tagIds.ToHashSet();
        var knownTagCount = await dbContext.Tags
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId && distinctTagIds.Contains(x.TagId))
            .CountAsync(cancellationToken);

        return knownTagCount == distinctTagIds.Count;
    }

    private static string? TryParseArchivedFilter(string? rawFilter, out ArchivedItemsFilter archivedFilter)
    {
        if (string.IsNullOrWhiteSpace(rawFilter))
        {
            archivedFilter = ArchivedItemsFilter.ActiveOnly;
            return null;
        }

        if (Enum.TryParse<ArchivedItemsFilter>(rawFilter.Trim(), ignoreCase: true, out archivedFilter))
        {
            return null;
        }

        archivedFilter = ArchivedItemsFilter.ActiveOnly;
        return "archived must be ActiveOnly, IncludeArchived, or ArchivedOnly.";
    }

    private static string? ValidateCreateRequest(
        CreateItemRequest request,
        out NormalizedCreateItemRequest normalizedRequest)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            normalizedRequest = default;
            return "name is required.";
        }

        if (name.Length > 120)
        {
            normalizedRequest = default;
            return "name must be 120 characters or fewer.";
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        if (description?.Length > 1000)
        {
            normalizedRequest = default;
            return "description must be 1000 characters or fewer.";
        }

        if (request.CategoryId == Guid.Empty)
        {
            normalizedRequest = default;
            return "categoryId is required.";
        }

        if (request.UnitId == Guid.Empty)
        {
            normalizedRequest = default;
            return "unitId is required.";
        }

        if (request.BaseValueMinor < 0)
        {
            normalizedRequest = default;
            return "baseValueMinor must be greater than or equal to 0.";
        }

        if (request.DefaultListPriceMinor is < 0)
        {
            normalizedRequest = default;
            return "defaultListPriceMinor must be greater than or equal to 0.";
        }

        if (!TryNormalizeTagIds(request.TagIds, out var normalizedTagIds, out var tagValidationError))
        {
            normalizedRequest = default;
            return tagValidationError;
        }

        normalizedRequest = new NormalizedCreateItemRequest(
            name,
            description,
            request.CategoryId,
            request.UnitId,
            request.BaseValueMinor,
            request.DefaultListPriceMinor,
            request.Weight,
            request.ImageAssetId,
            normalizedTagIds);

        return null;
    }

    private static string? TryParseUpdateRequest(JsonElement requestBody, out UpdateItemPatch patch)
    {
        patch = new UpdateItemPatch();

        if (requestBody.ValueKind != JsonValueKind.Object)
        {
            return "request body must be a JSON object.";
        }

        var properties = requestBody
            .EnumerateObject()
            .ToDictionary(x => x.Name, x => x.Value, StringComparer.OrdinalIgnoreCase);

        if (properties.TryGetValue("name", out var nameElement))
        {
            if (nameElement.ValueKind is not JsonValueKind.String)
            {
                return "name must be a string.";
            }

            var normalizedName = nameElement.GetString()?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                return "name is required.";
            }

            if (normalizedName.Length > 120)
            {
                return "name must be 120 characters or fewer.";
            }

            patch.HasName = true;
            patch.Name = normalizedName;
        }

        if (properties.TryGetValue("description", out var descriptionElement))
        {
            if (descriptionElement.ValueKind == JsonValueKind.Null)
            {
                patch.HasDescription = true;
                patch.Description = null;
            }
            else if (descriptionElement.ValueKind == JsonValueKind.String)
            {
                var normalizedDescription = descriptionElement.GetString();
                normalizedDescription = string.IsNullOrWhiteSpace(normalizedDescription)
                    ? null
                    : normalizedDescription.Trim();

                if (normalizedDescription?.Length > 1000)
                {
                    return "description must be 1000 characters or fewer.";
                }

                patch.HasDescription = true;
                patch.Description = normalizedDescription;
            }
            else
            {
                return "description must be a string or null.";
            }
        }

        if (properties.TryGetValue("categoryId", out var categoryIdElement))
        {
            if (categoryIdElement.ValueKind is not JsonValueKind.String
                || !Guid.TryParse(categoryIdElement.GetString(), out var categoryId)
                || categoryId == Guid.Empty)
            {
                return "categoryId must be a non-empty GUID string.";
            }

            patch.HasCategoryId = true;
            patch.CategoryId = categoryId;
        }

        if (properties.TryGetValue("unitId", out var unitIdElement))
        {
            if (unitIdElement.ValueKind is not JsonValueKind.String
                || !Guid.TryParse(unitIdElement.GetString(), out var unitId)
                || unitId == Guid.Empty)
            {
                return "unitId must be a non-empty GUID string.";
            }

            patch.HasUnitId = true;
            patch.UnitId = unitId;
        }

        if (properties.TryGetValue("baseValueMinor", out var baseValueElement))
        {
            if (baseValueElement.ValueKind is not JsonValueKind.Number
                || !baseValueElement.TryGetInt64(out var baseValueMinor))
            {
                return "baseValueMinor must be an integer.";
            }

            if (baseValueMinor < 0)
            {
                return "baseValueMinor must be greater than or equal to 0.";
            }

            patch.HasBaseValueMinor = true;
            patch.BaseValueMinor = baseValueMinor;
        }

        if (properties.TryGetValue("defaultListPriceMinor", out var defaultListPriceElement))
        {
            if (defaultListPriceElement.ValueKind == JsonValueKind.Null)
            {
                patch.HasDefaultListPriceMinor = true;
                patch.DefaultListPriceMinor = null;
            }
            else if (defaultListPriceElement.ValueKind == JsonValueKind.Number
                     && defaultListPriceElement.TryGetInt64(out var defaultListPriceMinor))
            {
                if (defaultListPriceMinor < 0)
                {
                    return "defaultListPriceMinor must be greater than or equal to 0.";
                }

                patch.HasDefaultListPriceMinor = true;
                patch.DefaultListPriceMinor = defaultListPriceMinor;
            }
            else
            {
                return "defaultListPriceMinor must be an integer or null.";
            }
        }

        if (properties.TryGetValue("weight", out var weightElement))
        {
            if (weightElement.ValueKind == JsonValueKind.Null)
            {
                patch.HasWeight = true;
                patch.Weight = null;
            }
            else if (weightElement.ValueKind == JsonValueKind.Number
                     && weightElement.TryGetDecimal(out var weight))
            {
                patch.HasWeight = true;
                patch.Weight = weight;
            }
            else
            {
                return "weight must be a number or null.";
            }
        }

        if (properties.TryGetValue("imageAssetId", out var imageAssetIdElement))
        {
            if (imageAssetIdElement.ValueKind == JsonValueKind.Null)
            {
                patch.HasImageAssetId = true;
                patch.ImageAssetId = null;
            }
            else if (imageAssetIdElement.ValueKind is JsonValueKind.String
                && Guid.TryParse(imageAssetIdElement.GetString(), out var imageAssetId)
                && imageAssetId != Guid.Empty)
            {
                patch.HasImageAssetId = true;
                patch.ImageAssetId = imageAssetId;
            }
            else
            {
                return "imageAssetId must be a non-empty GUID string or null.";
            }
        }

        if (properties.TryGetValue("tagIds", out var tagIdsElement))
        {
            if (tagIdsElement.ValueKind == JsonValueKind.Null)
            {
                patch.HasTagIds = true;
                patch.TagIds = [];
            }
            else if (tagIdsElement.ValueKind == JsonValueKind.Array)
            {
                var normalizedTagIds = new List<Guid>();
                foreach (var tagIdElement in tagIdsElement.EnumerateArray())
                {
                    if (tagIdElement.ValueKind is not JsonValueKind.String
                        || !Guid.TryParse(tagIdElement.GetString(), out var tagId)
                        || tagId == Guid.Empty)
                    {
                        return "tagIds must contain non-empty GUID strings.";
                    }

                    normalizedTagIds.Add(tagId);
                }

                if (!TryNormalizeTagIds(normalizedTagIds, out var distinctTagIds, out var tagValidationError))
                {
                    return tagValidationError;
                }

                patch.HasTagIds = true;
                patch.TagIds = distinctTagIds;
            }
            else
            {
                return "tagIds must be an array of GUID strings or null.";
            }
        }

        return null;
    }

    private static bool TryNormalizeTagIds(
        IReadOnlyList<Guid>? input,
        out IReadOnlyList<Guid> normalizedTagIds,
        out string validationError)
    {
        validationError = string.Empty;

        if (input is null || input.Count == 0)
        {
            normalizedTagIds = [];
            return true;
        }

        var seen = new HashSet<Guid>();
        var tagIds = new List<Guid>(input.Count);
        foreach (var tagId in input)
        {
            if (tagId == Guid.Empty)
            {
                normalizedTagIds = [];
                validationError = "tagIds cannot contain empty GUIDs.";
                return false;
            }

            if (!seen.Add(tagId))
            {
                normalizedTagIds = [];
                validationError = "tagIds cannot contain duplicates.";
                return false;
            }

            tagIds.Add(tagId);
        }

        normalizedTagIds = tagIds;
        return true;
    }

    private readonly record struct NormalizedCreateItemRequest(
        string Name,
        string? Description,
        Guid CategoryId,
        Guid UnitId,
        long BaseValueMinor,
        long? DefaultListPriceMinor,
        decimal? Weight,
        Guid? ImageAssetId,
        IReadOnlyList<Guid> TagIds);

    private sealed class UpdateItemPatch
    {
        public bool HasName { get; set; }

        public string? Name { get; set; }

        public bool HasDescription { get; set; }

        public string? Description { get; set; }

        public bool HasCategoryId { get; set; }

        public Guid? CategoryId { get; set; }

        public bool HasUnitId { get; set; }

        public Guid? UnitId { get; set; }

        public bool HasBaseValueMinor { get; set; }

        public long? BaseValueMinor { get; set; }

        public bool HasDefaultListPriceMinor { get; set; }

        public long? DefaultListPriceMinor { get; set; }

        public bool HasWeight { get; set; }

        public decimal? Weight { get; set; }

        public bool HasImageAssetId { get; set; }

        public Guid? ImageAssetId { get; set; }

        public bool HasTagIds { get; set; }

        public IReadOnlyList<Guid>? TagIds { get; set; }

        public bool HasAnyChanges
            => HasName
               || HasDescription
               || HasCategoryId
               || HasUnitId
               || HasBaseValueMinor
               || HasDefaultListPriceMinor
               || HasWeight
               || HasImageAssetId
               || HasTagIds;
    }
}
