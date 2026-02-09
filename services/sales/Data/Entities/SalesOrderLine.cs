namespace DndApp.Sales.Data.Entities;

public sealed class SalesOrderLine
{
    public Guid SaleLineId { get; set; }

    public Guid SaleId { get; set; }

    public Guid ItemId { get; set; }

    public decimal Quantity { get; set; }

    public long UnitSoldPriceMinor { get; set; }

    public long? UnitTrueValueMinor { get; set; }

    public long DiscountMinor { get; set; }

    public string? Notes { get; set; }

    public long LineSubtotalMinor { get; set; }

    public SalesOrder? Sale { get; set; }
}
