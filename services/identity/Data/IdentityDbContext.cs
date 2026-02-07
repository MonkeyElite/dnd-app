using DndApp.Identity.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Identity.Data;

public sealed class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<CampaignMembership> CampaignMemberships => Set<CampaignMembership>();

    public DbSet<Invite> Invites => Set<Invite>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.UserId);
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<CampaignMembership>(entity =>
        {
            entity.ToTable("CampaignMemberships");
            entity.HasKey(x => new { x.CampaignId, x.UserId });
            entity.Property(x => x.Role).IsRequired();
            entity.HasIndex(x => x.UserId);
            entity
                .HasOne(x => x.User)
                .WithMany(x => x.CampaignMemberships)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Invite>(entity =>
        {
            entity.ToTable("Invites");
            entity.HasKey(x => x.InviteId);
            entity.Property(x => x.CodeHash).IsRequired();
            entity.Property(x => x.Role).IsRequired();
            entity.Property(x => x.MaxUses).IsRequired().HasDefaultValue(1);
            entity.Property(x => x.Uses).IsRequired().HasDefaultValue(0);
            entity.Property(x => x.CreatedByUserId).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => x.CodeHash);
            entity
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
