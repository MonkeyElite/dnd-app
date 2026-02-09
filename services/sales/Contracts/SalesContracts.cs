using System.Text.Json;

namespace DndApp.Sales.Contracts;

public sealed record CreateSaleRequest(
    int SoldWorldDay,
    Guid StorageLocationId,
    Guid? CustomerId,
    string? Notes);

public sealed record CreateSaleResponse(Guid SaleId);

public sealed record UpdateSaleRequest(
    int SoldWorldDay,
    Guid StorageLocationId,
    Guid? CustomerId,
    string? Notes,
    IReadOnlyList<UpdateSaleLineRequest> Lines,
    IReadOnlyList<UpdateSalePaymentRequest> Payments);

public sealed record UpdateSaleLineRequest(
    Guid? SaleLineId,
    Guid ItemId,
    decimal Quantity,
    long UnitSoldPriceMinor,
    long? UnitTrueValueMinor,
    long DiscountMinor,
    string? Notes);

public sealed record UpdateSalePaymentRequest(
    Guid? PaymentId,
    string Method,
    long AmountMinor,
    JsonElement? Details);

public sealed record UpdateSaleResponse(bool Updated);

public sealed record CompleteSaleRequest;

public sealed record CompleteSaleResponse(string Status);

public sealed record VoidSaleRequest(string Reason);

public sealed record VoidSaleResponse(string Status);

public sealed record SaleListItemDto(
    Guid SaleId,
    string Status,
    int SoldWorldDay,
    Guid? CustomerId,
    Guid StorageLocationId,
    long TotalMinor);

public sealed record SaleTotalsDto(
    long SubtotalMinor,
    long DiscountTotalMinor,
    long TaxTotalMinor,
    long TotalMinor);

public sealed record SaleLineDto(
    Guid SaleLineId,
    Guid ItemId,
    decimal Quantity,
    long UnitSoldPriceMinor,
    long? UnitTrueValueMinor,
    long DiscountMinor,
    string? Notes,
    long LineSubtotalMinor);

public sealed record SalePaymentDto(
    Guid PaymentId,
    string Method,
    long AmountMinor,
    JsonElement? Details);

public sealed record SaleDetailDto(
    Guid SaleId,
    Guid CampaignId,
    string Status,
    int SoldWorldDay,
    Guid? CustomerId,
    Guid StorageLocationId,
    string? Notes,
    SaleTotalsDto Totals,
    IReadOnlyList<SaleLineDto> Lines,
    IReadOnlyList<SalePaymentDto> Payments);
