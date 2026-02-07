namespace DndShop.Contracts;

/// <summary>
/// Represents a monetary value using minor currency units.
/// </summary>
public sealed class MoneyDto
{
    /// <summary>
    /// Gets the amount in minor units, such as cents.
    /// </summary>
    public long AmountMinor { get; init; }

    /// <summary>
    /// Gets the three-letter ISO 4217 currency code.
    /// </summary>
    public string CurrencyCode { get; init; } = string.Empty;
}
