using DndApp.Identity.Data;
using DndApp.Identity.Data.Entities;
using DndApp.Identity.Options;
using DndApp.Identity.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DndApp.Identity.Seeding;

public sealed class DevSeeder(
    IdentityDbContext dbContext,
    IPasswordHasher passwordHasher,
    IOptions<DevSeedOptions> options,
    ILogger<DevSeeder> logger)
{
    private readonly DevSeedOptions _options = options.Value;

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return;
        }

        if (await dbContext.Users.AnyAsync(x => x.IsPlatformAdmin, cancellationToken))
        {
            return;
        }

        var username = _options.AdminUsername.Trim();
        var password = _options.AdminPassword;
        var displayName = _options.AdminDisplayName.Trim();

        if (string.IsNullOrWhiteSpace(username) || username.Length > 50)
        {
            throw new InvalidOperationException("DevSeed:AdminUsername must be 1-50 characters.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("DevSeed:AdminPassword is required when DevSeed:Enabled is true.");
        }

        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 50)
        {
            throw new InvalidOperationException("DevSeed:AdminDisplayName must be 1-50 characters.");
        }

        if (await dbContext.Users.AnyAsync(x => x.Username == username, cancellationToken))
        {
            throw new InvalidOperationException(
                "Dev seed admin username is already used by a non-platform-admin user.");
        }

        dbContext.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            PasswordHash = passwordHasher.HashPassword(password),
            DisplayName = displayName,
            IsPlatformAdmin = true,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Dev seed created platform admin user '{Username}'.", username);
    }
}
