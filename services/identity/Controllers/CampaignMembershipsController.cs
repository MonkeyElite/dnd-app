using System.IdentityModel.Tokens.Jwt;
using DndApp.Identity.Authorization;
using DndApp.Identity.Contracts;
using DndApp.Identity.Data;
using DndApp.Identity.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Identity.Controllers;

[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class CampaignMembershipsController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpPost("campaign-memberships")]
    public async Task<IActionResult> UpsertAsync(
        [FromBody] UpsertCampaignMembershipRequest request,
        CancellationToken cancellationToken)
    {
        if (!RoleRules.TryNormalizeRole(request.Role, out var normalizedRole))
        {
            return BadRequest(new ErrorResponse("Role is invalid."));
        }

        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        if (!await CanManageCampaignMembershipsAsync(request.CampaignId, requestingUserId, cancellationToken))
        {
            return Forbid();
        }

        var targetUserExists = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(x => x.UserId == request.UserId, cancellationToken);

        if (!targetUserExists)
        {
            return NotFound(new ErrorResponse("User not found."));
        }

        var membership = await dbContext.CampaignMemberships
            .SingleOrDefaultAsync(
                x => x.CampaignId == request.CampaignId && x.UserId == request.UserId,
                cancellationToken);

        if (membership is null)
        {
            membership = new CampaignMembership
            {
                CampaignId = request.CampaignId,
                UserId = request.UserId,
                Role = normalizedRole
            };

            dbContext.CampaignMemberships.Add(membership);
        }
        else
        {
            membership.Role = normalizedRole;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new CampaignMembershipResponse(membership.CampaignId, membership.UserId, membership.Role));
    }

    [HttpGet("me/campaign-memberships")]
    public async Task<IActionResult> GetMineAsync(CancellationToken cancellationToken)
    {
        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        var memberships = await dbContext.CampaignMemberships
            .AsNoTracking()
            .Where(x => x.UserId == requestingUserId)
            .OrderBy(x => x.CampaignId)
            .Select(x => new { x.CampaignId, x.Role })
            .ToListAsync(cancellationToken);

        var response = memberships
            .Select(x =>
            {
                var role = RoleRules.TryNormalizeRole(x.Role, out var normalizedRole)
                    ? normalizedRole
                    : x.Role;

                return new MyCampaignMembershipResponse(x.CampaignId, role);
            })
            .ToList();

        return Ok(response);
    }

    [HttpGet("campaigns/{campaignId:guid}/members/me")]
    public async Task<IActionResult> GetMyMembershipForCampaignAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        var membership = await dbContext.CampaignMemberships
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId && x.UserId == requestingUserId)
            .Select(x => new { x.CampaignId, x.UserId, x.Role })
            .SingleOrDefaultAsync(cancellationToken);

        if (membership is null)
        {
            return NotFound(new ErrorResponse("Membership not found."));
        }

        var role = RoleRules.TryNormalizeRole(membership.Role, out var normalizedRole)
            ? normalizedRole
            : membership.Role;

        return Ok(new MyCampaignMemberRoleResponse(membership.CampaignId, membership.UserId, role));
    }

    [HttpGet("campaigns/{campaignId:guid}/members")]
    public async Task<IActionResult> GetCampaignMembersAsync(Guid campaignId, CancellationToken cancellationToken)
    {
        if (!TryGetRequestingUserId(out var requestingUserId))
        {
            return Unauthorized();
        }

        if (!await CanReadCampaignMembersAsync(campaignId, requestingUserId, cancellationToken))
        {
            return Forbid();
        }

        var members = await dbContext.CampaignMemberships
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId)
            .Join(
                dbContext.Users.AsNoTracking(),
                membership => membership.UserId,
                user => user.UserId,
                (membership, user) => new
                {
                    membership.CampaignId,
                    membership.UserId,
                    membership.Role,
                    user.Username,
                    user.DisplayName,
                    user.IsPlatformAdmin
                })
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.Username)
            .ToListAsync(cancellationToken);

        var response = members
            .Select(member =>
            {
                var role = RoleRules.TryNormalizeRole(member.Role, out var normalizedRole)
                    ? normalizedRole
                    : member.Role;

                return new CampaignMemberSummaryResponse(
                    member.CampaignId,
                    member.UserId,
                    member.Username,
                    member.DisplayName,
                    role,
                    member.IsPlatformAdmin);
            })
            .ToList();

        return Ok(response);
    }

    private async Task<bool> CanReadCampaignMembersAsync(
        Guid campaignId,
        Guid requestingUserId,
        CancellationToken cancellationToken)
    {
        var isPlatformAdmin = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.UserId == requestingUserId)
            .Select(x => (bool?)x.IsPlatformAdmin)
            .SingleOrDefaultAsync(cancellationToken);

        if (isPlatformAdmin is true)
        {
            return true;
        }

        return await dbContext.CampaignMemberships
            .AsNoTracking()
            .AnyAsync(
                x => x.CampaignId == campaignId
                     && x.UserId == requestingUserId,
                cancellationToken);
    }

    private async Task<bool> CanManageCampaignMembershipsAsync(
        Guid campaignId,
        Guid requestingUserId,
        CancellationToken cancellationToken)
    {
        var isPlatformAdmin = await dbContext.Users
            .AsNoTracking()
            .Where(x => x.UserId == requestingUserId)
            .Select(x => (bool?)x.IsPlatformAdmin)
            .SingleOrDefaultAsync(cancellationToken);

        if (isPlatformAdmin is true)
        {
            return true;
        }

        var requesterRole = await dbContext.CampaignMemberships
            .AsNoTracking()
            .Where(x => x.CampaignId == campaignId && x.UserId == requestingUserId)
            .Select(x => x.Role)
            .SingleOrDefaultAsync(cancellationToken);

        return RoleRules.TryNormalizeRole(requesterRole, out var normalizedRole)
            && RoleRules.CanManageCampaignMembers(normalizedRole);
    }

    private bool TryGetRequestingUserId(out Guid userId)
    {
        var subjectValue = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(subjectValue, out userId);
    }
}
