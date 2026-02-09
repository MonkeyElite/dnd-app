using System.Text.Json;

namespace DndApp.Bff.Contracts;

public sealed record SalesCreateRequest(
    int SoldWorldDay,
    Guid StorageLocationId,
    Guid? CustomerId,
    string? Notes);

public sealed record SalesCreateResponse(Guid SaleId);

public sealed record SalesUpdateRequest(
    int SoldWorldDay,
    Guid StorageLocationId,
    Guid? CustomerId,
    string? Notes,
    IReadOnlyList<SalesUpdateLineRequest> Lines,
    IReadOnlyList<SalesUpdatePaymentRequest> Payments);

public sealed record SalesUpdateLineRequest(
    Guid? SaleLineId,
    Guid ItemId,
    decimal Quantity,
    long UnitSoldPriceMinor,
    long? UnitTrueValueMinor,
    long DiscountMinor,
    string? Notes);

public sealed record SalesUpdatePaymentRequest(
    Guid? PaymentId,
    string Method,
    long AmountMinor,
    JsonElement? Details);

public sealed record SalesUpdateResponse(bool Updated);

public sealed record SalesCompleteRequest;

public sealed record SalesCompleteResponse(string Status);

public sealed record SalesVoidRequest(string Reason);

public sealed record SalesVoidResponse(string Status);

public sealed record SalesListItemDto(
    Guid SaleId,
    string Status,
    int SoldWorldDay,
    Guid? CustomerId,
    Guid StorageLocationId,
    long TotalMinor);

public sealed record SalesTotalsDto(
    long SubtotalMinor,
    long DiscountTotalMinor,
    long TaxTotalMinor,
    long TotalMinor);

public sealed record SalesLineDto(
    Guid SaleLineId,
    Guid ItemId,
    decimal Quantity,
    long UnitSoldPriceMinor,
    long? UnitTrueValueMinor,
    long DiscountMinor,
    string? Notes,
    long LineSubtotalMinor);

public sealed record SalesPaymentDto(
    Guid PaymentId,
    string Method,
    long AmountMinor,
    JsonElement? Details);

public sealed record SalesDetailDto(
    Guid SaleId,
    Guid CampaignId,
    string Status,
    int SoldWorldDay,
    Guid? CustomerId,
    Guid StorageLocationId,
    string? Notes,
    SalesTotalsDto Totals,
    IReadOnlyList<SalesLineDto> Lines,
    IReadOnlyList<SalesPaymentDto> Payments);

public sealed record CreateSaleActionRequest(
    Guid CampaignId,
    int SoldWorldDay,
    Guid StorageLocationId,
    Guid? CustomerId,
    string? Notes);

public sealed record UpdateSaleActionRequest(
    Guid CampaignId,
    int SoldWorldDay,
    Guid StorageLocationId,
    Guid? CustomerId,
    string? Notes,
    IReadOnlyList<UpdateSaleLineActionRequest> Lines,
    IReadOnlyList<UpdateSalePaymentActionRequest> Payments);

public sealed record UpdateSaleLineActionRequest(
    Guid? SaleLineId,
    Guid ItemId,
    decimal Quantity,
    long UnitSoldPriceMinor,
    long? UnitTrueValueMinor,
    long DiscountMinor,
    string? Notes);

public sealed record UpdateSalePaymentActionRequest(
    Guid? PaymentId,
    string Method,
    long AmountMinor,
    JsonElement? Details);

public sealed record CompleteSaleActionRequest(Guid CampaignId);

public sealed record VoidSaleActionRequest(Guid CampaignId, string Reason);

public sealed record SalesPageFilterCustomerDto(Guid CustomerId, string Name);

public sealed record SalesPageFiltersDto(IReadOnlyList<SalesPageFilterCustomerDto> Customers);

public sealed record SalesPageRowDto(
    Guid SaleId,
    string Status,
    int SoldWorldDay,
    string? CustomerName,
    long TotalMinor);

public sealed record SalesPageResponse(
    Guid CampaignId,
    string CurrencyCode,
    SalesPageFiltersDto Filters,
    IReadOnlyList<SalesPageRowDto> Sales);

public sealed record SalePageFilterStorageLocationDto(Guid StorageLocationId, string Name);

public sealed record SalePageFiltersDto(
    IReadOnlyList<SalesPageFilterCustomerDto> Customers,
    IReadOnlyList<SalePageFilterStorageLocationDto> StorageLocations);

public sealed record SalePageResponse(
    Guid CampaignId,
    string CurrencyCode,
    SalesDetailDto Sale,
    SalePageFiltersDto Filters);
