using DndApp.Campaign.Data.Entities;
using Microsoft.EntityFrameworkCore;
using CampaignEntity = DndApp.Campaign.Data.Entities.Campaign;

namespace DndApp.Campaign.Data;

public sealed class CampaignDbContext(DbContextOptions<CampaignDbContext> options) : DbContext(options)
{
    public DbSet<CampaignEntity> Campaigns => Set<CampaignEntity>();

    public DbSet<CalendarConfig> CalendarConfigs => Set<CalendarConfig>();

    public DbSet<CurrencyConfig> CurrencyConfigs => Set<CurrencyConfig>();

    public DbSet<Place> Places => Set<Place>();

    public DbSet<NpcCustomer> NpcCustomers => Set<NpcCustomer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CampaignEntity>(entity =>
        {
            entity.ToTable("Campaigns");
            entity.HasKey(x => x.CampaignId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(500);
            entity.Property(x => x.CreatedByUserId).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.CreatedByUserId);
        });

        modelBuilder.Entity<CalendarConfig>(entity =>
        {
            entity.ToTable("CalendarConfig");
            entity.HasKey(x => x.CampaignId);
            entity.Property(x => x.WeekLength).IsRequired();
            entity.Property(x => x.MonthsJson).HasColumnType("jsonb").IsRequired();
            entity
                .HasOne(x => x.Campaign)
                .WithOne(x => x.CalendarConfig)
                .HasForeignKey<CalendarConfig>(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CurrencyConfig>(entity =>
        {
            entity.ToTable("CurrencyConfig");
            entity.HasKey(x => x.CampaignId);
            entity.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
            entity.Property(x => x.MinorUnitName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.MajorUnitName).HasMaxLength(50).IsRequired();
            entity.Property(x => x.DenominationsJson).HasColumnType("jsonb").IsRequired();
            entity
                .HasOne(x => x.Campaign)
                .WithOne(x => x.CurrencyConfig)
                .HasForeignKey<CurrencyConfig>(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Place>(entity =>
        {
            entity.ToTable("Places");
            entity.HasKey(x => x.PlaceId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => new { x.CampaignId, x.Name });
            entity
                .HasOne(x => x.Campaign)
                .WithMany(x => x.Places)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NpcCustomer>(entity =>
        {
            entity.ToTable("NpcCustomers");
            entity.HasKey(x => x.CustomerId);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.TagsJson).HasColumnName("Tags").HasColumnType("jsonb").IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => new { x.CampaignId, x.Name });
            entity
                .HasOne(x => x.Campaign)
                .WithMany(x => x.NpcCustomers)
                .HasForeignKey(x => x.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
