namespace DndApp.Sales.Data.Entities;

public sealed class SalesOrder
{
    public Guid SaleId { get; set; }

    public Guid CampaignId { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? CustomerId { get; set; }

    public Guid StorageLocationId { get; set; }

    public int SoldWorldDay { get; set; }

    public long SubtotalMinor { get; set; }

    public long DiscountTotalMinor { get; set; }

    public long TaxTotalMinor { get; set; }

    public long TotalMinor { get; set; }

    public string? Notes { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<SalesOrderLine> Lines { get; set; } = [];

    public ICollection<SalesPayment> Payments { get; set; } = [];
}
