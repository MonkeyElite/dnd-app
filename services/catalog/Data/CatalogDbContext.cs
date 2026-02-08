using DndApp.Catalog.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Catalog.Data;

public sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Unit> Units => Set<Unit>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Item> Items => Set<Item>();

    public DbSet<ItemTag> ItemTags => Set<ItemTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(x => x.CategoryId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => new { x.CampaignId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.ToTable("Units");
            entity.HasKey(x => x.UnitId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => new { x.CampaignId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(x => x.TagId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => new { x.CampaignId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.ToTable("Items", table =>
            {
                table.HasCheckConstraint("CK_Items_BaseValueMinor_NonNegative", "\"BaseValueMinor\" >= 0");
                table.HasCheckConstraint(
                    "CK_Items_DefaultListPriceMinor_NonNegative",
                    "\"DefaultListPriceMinor\" IS NULL OR \"DefaultListPriceMinor\" >= 0");
            });

            entity.HasKey(x => x.ItemId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CategoryId).IsRequired();
            entity.Property(x => x.UnitId).IsRequired();
            entity.Property(x => x.BaseValueMinor).IsRequired();
            entity.Property(x => x.DefaultListPriceMinor);
            entity.Property(x => x.Weight).HasColumnType("numeric(10,3)");
            entity.Property(x => x.ImageAssetId);
            entity.Property(x => x.IsArchived).IsRequired().HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => new { x.CampaignId, x.Name });

            entity
                .HasOne(x => x.Category)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(x => x.Unit)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.UnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ItemTag>(entity =>
        {
            entity.ToTable("ItemTags");
            entity.HasKey(x => new { x.ItemId, x.TagId });
            entity.Property(x => x.ItemId).IsRequired();
            entity.Property(x => x.TagId).IsRequired();
            entity.HasIndex(x => x.TagId);

            entity
                .HasOne(x => x.Item)
                .WithMany(x => x.ItemTags)
                .HasForeignKey(x => x.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity
                .HasOne(x => x.Tag)
                .WithMany(x => x.ItemTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
