namespace DndApp.Campaign.Data.Entities;

public sealed class CurrencyConfig
{
    public Guid CampaignId { get; set; }

    public string CurrencyCode { get; set; } = string.Empty;

    public string MinorUnitName { get; set; } = string.Empty;

    public string MajorUnitName { get; set; } = string.Empty;

    public string DenominationsJson { get; set; } = "[]";

    public Campaign? Campaign { get; set; }
}
