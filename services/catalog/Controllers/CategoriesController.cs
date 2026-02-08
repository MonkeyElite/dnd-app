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
[Route("campaigns/{campaignId:guid}/categories")]
public sealed class CategoriesController(CatalogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var categories = await dbContext.Categories
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId)
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto(x.CategoryId, x.CampaignId, x.Name))
            .ToListAsync(cancellationToken);

        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request, out var normalizedName);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var nameExists = await dbContext.Categories
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId == campaignId && x.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            return Conflict(new ErrorResponse("Category name already exists for this campaign."));
        }

        var category = new Category
        {
            CategoryId = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = normalizedName
        };

        dbContext.Categories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateCategoryResponse(category.CategoryId));
    }

    private static string? ValidateRequest(CreateCategoryRequest request, out string normalizedName)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            normalizedName = string.Empty;
            return "name is required.";
        }

        if (name.Length > 100)
        {
            normalizedName = string.Empty;
            return "name must be 100 characters or fewer.";
        }

        normalizedName = name;
        return null;
    }
}
