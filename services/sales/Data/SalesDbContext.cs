using DndApp.Sales.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace DndApp.Sales.Data;

public sealed class SalesDbContext(DbContextOptions<SalesDbContext> options) : DbContext(options)
{
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    public DbSet<SalesPayment> SalesPayments => Set<SalesPayment>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.ToTable("SalesOrders");
            entity.HasKey(x => x.SaleId);
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.CustomerId);
            entity.Property(x => x.StorageLocationId).IsRequired();
            entity.Property(x => x.SoldWorldDay).IsRequired();
            entity.Property(x => x.SubtotalMinor).IsRequired();
            entity.Property(x => x.DiscountTotalMinor).IsRequired();
            entity.Property(x => x.TaxTotalMinor).IsRequired();
            entity.Property(x => x.TotalMinor).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.CreatedByUserId).IsRequired();
            entity.Property(x => x.CompletedAt);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.Property(x => x.UpdatedAt).IsRequired();
            entity.HasIndex(x => x.CampaignId);
            entity.HasIndex(x => new { x.CampaignId, x.SoldWorldDay });
        });

        modelBuilder.Entity<SalesOrderLine>(entity =>
        {
            entity.ToTable("SalesOrderLines");
            entity.HasKey(x => x.SaleLineId);
            entity.Property(x => x.SaleId).IsRequired();
            entity.Property(x => x.ItemId).IsRequired();
            entity.Property(x => x.Quantity).HasColumnType("numeric(18,3)").IsRequired();
            entity.Property(x => x.UnitSoldPriceMinor).IsRequired();
            entity.Property(x => x.UnitTrueValueMinor);
            entity.Property(x => x.DiscountMinor).HasDefaultValue(0L).IsRequired();
            entity.Property(x => x.Notes).HasMaxLength(500);
            entity.Property(x => x.LineSubtotalMinor).IsRequired();
            entity.HasIndex(x => x.SaleId);
            entity.HasIndex(x => x.ItemId);

            entity
                .HasOne(x => x.Sale)
                .WithMany(x => x.Lines)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SalesPayment>(entity =>
        {
            entity.ToTable("SalesPayments");
            entity.HasKey(x => x.PaymentId);
            entity.Property(x => x.SaleId).IsRequired();
            entity.Property(x => x.Method).HasMaxLength(50).IsRequired();
            entity.Property(x => x.AmountMinor).IsRequired();
            entity.Property(x => x.DetailsJson).HasColumnType("jsonb");
            entity.HasIndex(x => x.SaleId);

            entity
                .HasOne(x => x.Sale)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(x => x.OutboxMessageId);
            entity.Property(x => x.OccurredAt).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(120).IsRequired();
            entity.Property(x => x.AggregateId).IsRequired();
            entity.Property(x => x.CampaignId).IsRequired();
            entity.Property(x => x.CorrelationId).IsRequired();
            entity.Property(x => x.PayloadJson).HasColumnType("jsonb").IsRequired();
            entity.Property(x => x.PublishedAt);
            entity.Property(x => x.PublishAttempts).HasDefaultValue(0).IsRequired();
            entity.Property(x => x.LastError);
            entity.HasIndex(x => x.PublishedAt);
            entity.HasIndex(x => new { x.Type, x.OccurredAt });
        });
    }
}
