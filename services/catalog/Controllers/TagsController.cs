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
[Route("campaigns/{campaignId:guid}/tags")]
public sealed class TagsController(CatalogDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        var tags = await dbContext.Tags
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId)
            .OrderBy(x => x.Name)
            .Select(x => new TagDto(x.TagId, x.CampaignId, x.Name))
            .ToListAsync(cancellationToken);

        return Ok(tags);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRequest(request, out var normalizedName);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var nameExists = await dbContext.Tags
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId == campaignId && x.Name == normalizedName, cancellationToken);

        if (nameExists)
        {
            return Conflict(new ErrorResponse("Tag name already exists for this campaign."));
        }

        var tag = new Tag
        {
            TagId = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = normalizedName
        };

        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateTagResponse(tag.TagId));
    }

    private static string? ValidateRequest(CreateTagRequest request, out string normalizedName)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            normalizedName = string.Empty;
            return "name is required.";
        }

        if (name.Length > 50)
        {
            normalizedName = string.Empty;
            return "name must be 50 characters or fewer.";
        }

        normalizedName = name;
        return null;
    }
}
