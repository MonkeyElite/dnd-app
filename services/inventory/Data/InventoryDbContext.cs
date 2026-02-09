using DndApp.Inventory.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Inventory.Data;

public sealed class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<StorageLocation> StorageLocations => Set<StorageLocation>();

    public DbSet<InventoryLot> InventoryLots => Set<InventoryLot>();

    public DbSet<InventoryAdjustment> InventoryAdjustments => Set<InventoryAdjustment>();

    public DbSet<ProcessedEvent> ProcessedEvents => Set<ProcessedEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StorageLocation>(entity =>
        {
            entity.ToTable("StorageLocations");
            entity.HasKey(x => x.StorageLocationId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.PlaceId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(50);
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => x.PlaceId);
            entity.HasIndex(x => new { x.CampaignId, x.Name }).IsUnique();
        });

        modelBuilder.Entity<InventoryLot>(entity =>
        {
            entity.ToTable("InventoryLots", table =>
            {
                table.HasCheckConstraint("CK_InventoryLots_QuantityOnHand_NonNegative", "\"QuantityOnHand\" >= 0");
                table.HasCheckConstraint("CK_InventoryLots_UnitCostMinor_NonNegative", "\"UnitCostMinor\" >= 0");
            });

            entity.HasKey(x => x.LotId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.ItemId).IsRequired();
            entity.Property(x => x.StorageLocationId).IsRequired();
            entity.Property(x => x.QuantityOnHand).HasColumnType("numeric(18,3)").IsRequired();
            entity.Property(x => x.UnitCostMinor).IsRequired();
            entity.Property(x => x.AcquiredWorldDay).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(200);
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => x.ItemId);
            entity.HasIndex(x => x.StorageLocationId);
            entity.HasIndex(x => new { x.CampaignId, x.ItemId, x.StorageLocationId, x.AcquiredWorldDay, x.CreatedAt });

            entity
                .HasOne(x => x.StorageLocation)
                .WithMany(x => x.Lots)
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<InventoryAdjustment>(entity =>
        {
            entity.ToTable("InventoryAdjustments");
            entity.HasKey(x => x.AdjustmentId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.ItemId).IsRequired();
            entity.Property(x => x.StorageLocationId).IsRequired();
            entity.Property(x => x.LotId);
            entity.Property(x => x.DeltaQuantity).HasColumnType("numeric(18,3)").IsRequired();
            entity.Property(x => x.Reason).HasMaxLength(50).IsRequired();
            entity.Property(x => x.WorldDay).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.ReferenceType).HasMaxLength(50);
            entity.Property(x => x.ReferenceId);
            entity.Property(x => x.CreatedByUserId).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => x.ItemId);
            entity.HasIndex(x => x.StorageLocationId);
            entity.HasIndex(x => x.LotId);
            entity.HasIndex(x => new { x.CampaignId, x.WorldDay, x.CreatedAt });

            entity
                .HasOne(x => x.StorageLocation)
                .WithMany(x => x.Adjustments)
                .HasForeignKey(x => x.StorageLocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity
                .HasOne(x => x.Lot)
                .WithMany(x => x.Adjustments)
                .HasForeignKey(x => x.LotId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ProcessedEvent>(entity =>
        {
            entity.ToTable("ProcessedEvents");
            entity.HasKey(x => x.EventId);
            entity.Property(x => x.EventId).IsRequired();
            entity.Property(x => x.ProcessedAt).IsRequired();
        });
    }
}
