namespace DndShop.Contracts;

/// <summary>
/// Defines reasons for stock adjustments.
/// </summary>
public enum AdjustmentReason
{
    Restock,
    Sale,
    Damage,
    Theft,
    Spoilage,
    ManualCorrection
}
