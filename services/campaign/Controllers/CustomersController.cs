using DndApp.Campaign.Contracts;
using DndApp.Campaign.Data;
using DndApp.Campaign.Data.Entities;
using DndApp.Campaign.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Campaign.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/customers")]
public sealed class CustomersController(CampaignDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(
        Guid campaignId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var query = dbContext.NpcCustomers
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{search.Trim()}%"));
        }

        var customers = await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var response = customers.Select(x => new CustomerDto(
            x.CustomerId,
            x.CampaignId,
            x.Name,
            x.Notes,
            JsonArraySerializer.Deserialize<string>(x.TagsJson)))
            .ToList();

        return Ok(response);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateCustomerRequest(request, out var normalizedTags);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var campaignExists = await dbContext.Campaigns
            .AsNoTracking()
            .AnyAsync(x => x.CampaignId == campaignId, cancellationToken);

        if (!campaignExists)
        {
            return NotFound(new ErrorResponse("Campaign not found."));
        }

        var customer = new NpcCustomer
        {
            CustomerId = Guid.NewGuid(),
            CampaignId = campaignId,
            Name = request.Name.Trim(),
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            TagsJson = JsonArraySerializer.Serialize(normalizedTags)
        };

        dbContext.NpcCustomers.Add(customer);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateCustomerResponse(customer.CustomerId));
    }

    private static string? ValidateCreateCustomerRequest(
        CreateCustomerRequest request,
        out IReadOnlyList<string> normalizedTags)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            normalizedTags = [];
            return "name is required.";
        }

        if (request.Name.Trim().Length > 100)
        {
            normalizedTags = [];
            return "name must be 100 characters or fewer.";
        }

        if (request.Notes?.Trim().Length > 500)
        {
            normalizedTags = [];
            return "notes must be 500 characters or fewer.";
        }

        var tags = new List<string>();
        if (request.Tags is not null)
        {
            foreach (var tag in request.Tags)
            {
                var normalizedTag = tag?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(normalizedTag))
                {
                    normalizedTags = [];
                    return "tags cannot contain empty values.";
                }

                tags.Add(normalizedTag);
            }
        }

        normalizedTags = tags;
        return null;
    }
}
