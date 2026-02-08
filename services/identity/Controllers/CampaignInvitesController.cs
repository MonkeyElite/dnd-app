using System.IdentityModel.Tokens.Jwt;
using DndApp.Identity.Authorization;
using DndApp.Identity.Contracts;
using DndApp.Identity.Data;
using DndApp.Identity.Data.Entities;
using DndApp.Identity.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Identity.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
[Route("campaigns/{campaignId:guid}/invites")]
public sealed class CampaignInvitesController(
    IdentityDbContext dbContext,
    IInviteCodeGenerator inviteCodeGenerator,
    IInviteCodeHasher inviteCodeHasher) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateAsync(
        Guid campaignId,
        [FromBody] CreateInviteRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateCreateInviteRequest(request, out var normalizedRole);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        if (!await CanManageInvitesAsync(campaignId, requestingUserId, cancellationToken))
        {
            return Forbid();
        }

        var now = DateTimeOffset.UtcNow;
        var code = inviteCodeGenerator.Generate();
        DateTimeOffset? expiresAt = request.ExpiresInDays is > 0
            ? now.AddDays(request.ExpiresInDays.Value)
            : null;
        var invite = new Invite
        {
            InviteId = Guid.NewGuid(),
            CampaignId = campaignId,
            CodeHash = inviteCodeHasher.Hash(code),
            Role = normalizedRole,
            MaxUses = request.MaxUses,
            Uses = 0,
            ExpiresAt = expiresAt,
            CreatedByUserId = requestingUserId,
            CreatedAt = now,
            RevokedAt = null
        };

        dbContext.Invites.Add(invite);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new CreateInviteResponse(invite.InviteId, code, invite.Role, invite.MaxUses, invite.ExpiresAt));
    }

    [HttpGet]
    public async Task<IActionResult> GetAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        if (!await CanManageInvitesAsync(campaignId, requestingUserId, cancellationToken))
        {
            return Forbid();
        }

        var invites = await dbContext.Invites
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new InviteSummaryResponse(
                x.InviteId,
                x.Role,
                x.Uses,
                x.MaxUses,
                x.ExpiresAt,
                x.RevokedAt,
                x.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(invites);
    }

    [HttpPost("{inviteId:guid}/revoke")]
    public async Task<IActionResult> RevokeAsync(Guid campaignId, Guid inviteId, CancellationToken cancellationToken)
    {
        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        if (!await CanManageInvitesAsync(campaignId, requestingUserId, cancellationToken))
        {
            return Forbid();
        }

        var invite = await dbContext.Invites
            .FirstOrDefaultAsync(x => x.CampaignId == campaignId && x.InviteId == inviteId, cancellationToken);

        if (invite is null)
        {
            return NotFound(new ErrorResponse("Invite not found."));
        }

        if (invite.RevokedAt is null)
        {
            invite.RevokedAt = DateTimeOffset.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return Ok(new RevokeInviteResponse(true));
    }

    private async Task<bool> CanManageInvitesAsync(Guid campaignId, Guid userId, CancellationToken cancellationToken)
    {
        var isPlatformAdmin = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => (bool?)x.IsPlatformAdmin)
            .FirstOrDefaultAsync(cancellationToken);

        if (isPlatformAdmin is true)
        {
            return true;
        }

        var role = await dbContext.CampaignMemberships
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId && x.UserId == userId)
            .Select(x => x.Role)
            .FirstOrDefaultAsync(cancellationToken);

        return RoleRules.TryNormalizeRole(role, out var normalizedRole)
            && RoleRules.CanManageInvites(normalizedRole);
    }

    private bool TryGetRequestingUserId(out Guid userId)
    {
        var subjectValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(subjectValue, out userId);
    }

    private static string? ValidateCreateInviteRequest(CreateInviteRequest request, out string normalizedRole)
    {
        if (!RoleRules.TryNormalizeRole(request.Role, out normalizedRole))
        {
            return "Role is invalid.";
        }

        if (!RoleRules.CanAssignInviteRole(normalizedRole))
        {
            return "Role must be one of Member, Treasurer, ReadOnly, or Admin.";
        }

        if (request.MaxUses <= 0)
        {
            return "maxUses must be greater than 0.";
        }

        if (request.ExpiresInDays is < 1)
        {
            return "expiresInDays must be at least 1 day when provided.";
        }

        return null;
    }
}
