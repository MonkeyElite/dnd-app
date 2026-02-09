namespace DndApp.Sales.Data.Entities;

public sealed class SalesPayment
{
    public Guid PaymentId { get; set; }

    public Guid SaleId { get; set; }

    public string Method { get; set; } = string.Empty;

    public long AmountMinor { get; set; }

    public string? DetailsJson { get; set; }

    public SalesOrder? Sale { get; set; }
}
