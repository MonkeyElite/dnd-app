namespace DndShop.Contracts;

/// <summary>
/// Declares event type names published by the sales domain.
/// </summary>
public static class SalesEventTypes
{
    public const string SaleCompletedV1 = "Sales.SaleCompleted.v1";
}

/// <summary>
/// Emitted when a draft sale is completed.
/// </summary>
public sealed class SaleCompletedEvent
{
    public Guid SaleId { get; init; }

    public Guid CampaignId { get; init; }

    public int SoldWorldDay { get; init; }

    public Guid StorageLocationId { get; init; }

    public Guid? CustomerId { get; init; }

    public MoneyDto Total { get; init; } = default!;

    public MoneyDto TaxTotal { get; init; } = default!;

    public List<SaleCompletedLine> Lines { get; init; } = [];
}

/// <summary>
/// Represents one sold item line on a completed sale event.
/// </summary>
public sealed class SaleCompletedLine
{
    public Guid ItemId { get; init; }

    public decimal Quantity { get; init; }

    public MoneyDto UnitSoldPrice { get; init; } = default!;

    public MoneyDto UnitTrueValue { get; init; } = default!;
}
