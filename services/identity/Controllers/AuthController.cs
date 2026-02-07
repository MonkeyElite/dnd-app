using System.Data;
using DndApp.Identity.Contracts;
using DndApp.Identity.Data;
using DndApp.Identity.Data.Entities;
using DndApp.Identity.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace DndApp.Identity.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IInviteCodeHasher inviteCodeHasher,
    IJwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginAsync([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var validationError = ValidateLoginRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var username = request.Username.Trim();
        var user = await dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Username == username, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Unauthorized(new ErrorResponse("Invalid username or password."));
        }

        return Ok(CreateAuthResponse(user));
    }

    [HttpPost("register-with-invite")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterWithInviteAsync(
        [FromBody] RegisterWithInviteRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = ValidateRegisterWithInviteRequest(request);
        if (validationError is not null)
        {
            return BadRequest(new ErrorResponse(validationError));
        }

        var username = request.Username.Trim();
        var displayName = request.DisplayName.Trim();
        var codeHash = inviteCodeHasher.Hash(request.InviteCode);
        var now = DateTimeOffset.UtcNow;

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var invite = await dbContext.Invites
            .Where(x => x.CodeHash == codeHash)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var inviteError = ValidateInvite(invite, now);
        if (inviteError is not null)
        {
            return BadRequest(new ErrorResponse(inviteError));
        }

        if (await dbContext.Users.AnyAsync(x => x.Username == username, cancellationToken))
        {
            return Conflict(new ErrorResponse("Username is already taken."));
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            DisplayName = displayName,
            CreatedAt = now
        };

        dbContext.Users.Add(user);
        dbContext.CampaignMemberships.Add(new CampaignMembership
        {
            CampaignId = invite!.CampaignId,
            UserId = user.UserId,
            Role = invite.Role
        });
        invite.Uses += 1;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            return Conflict(new ErrorResponse("Username is already taken."));
        }

        return Ok(CreateAuthResponse(user));
    }

    private AuthResponse CreateAuthResponse(User user)
    {
        var accessToken = jwtTokenService.CreateAccessToken(user);
        return new AuthResponse(
            AccessToken: accessToken,
            RefreshToken: null,
            User: new AuthUserResponse(user.UserId, user.Username, user.DisplayName));
    }

    private static string? ValidateLoginRequest(LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return "Username is required.";
        }

        if (request.Username.Trim().Length > 50)
        {
            return "Username must be 50 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return "Password is required.";
        }

        return null;
    }

    private static string? ValidateRegisterWithInviteRequest(RegisterWithInviteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InviteCode))
        {
            return "Invite code is required.";
        }

        if (request.InviteCode.Trim().Length > 64)
        {
            return "Invite code is too long.";
        }

        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return "Username is required.";
        }

        if (request.Username.Trim().Length > 50)
        {
            return "Username must be 50 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return "Display name is required.";
        }

        if (request.DisplayName.Trim().Length > 50)
        {
            return "Display name must be 50 characters or fewer.";
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return "Password is required.";
        }

        if (request.Password.Length < 8)
        {
            return "Password must be at least 8 characters.";
        }

        return null;
    }

    private static string? ValidateInvite(Invite? invite, DateTimeOffset now)
    {
        if (invite is null)
        {
            return "Invalid invite code.";
        }

        if (invite.RevokedAt is not null)
        {
            return "Invite has been revoked.";
        }

        if (invite.ExpiresAt is not null && invite.ExpiresAt <= now)
        {
            return "Invite has expired.";
        }

        if (invite.Uses >= invite.MaxUses)
        {
            return "Invite has reached the maximum number of uses.";
        }

        return null;
    }

    private static bool IsUniqueViolation(DbUpdateException exception)
        => exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
