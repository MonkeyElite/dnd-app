using DndApp.Media.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Media.Data;

public sealed class MediaDbContext(DbContextOptions<MediaDbContext> options) : DbContext(options)
{
    public DbSet<Asset> Assets => Set<Asset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.ToTable("Assets");
            entity.HasKey(x => x.AssetId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(200).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(200);
            entity.Property(x => x.SizeBytes);
            entity.Property(x => x.Bucket).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ObjectKey).HasMaxLength(600).IsRequired();
            entity.Property(x => x.Sha256).HasMaxLength(64);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => new { x.CampaignId, x.AssetId });
            entity.HasIndex(x => x.ObjectKey).IsUnique();
        });
    }
}
