import 'package:json_annotation/json_annotation.dart';

part 'sales_models.g.dart';

@JsonSerializable()
class SalesPageFilterCustomerDto {
  SalesPageFilterCustomerDto({
    required this.customerId,
    required this.name,
  });

  final String customerId;
  final String name;

  factory SalesPageFilterCustomerDto.fromJson(Map<String, dynamic> json) =>
      _$SalesPageFilterCustomerDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesPageFilterCustomerDtoToJson(this);
}

@JsonSerializable()
class SalesPageFiltersDto {
  SalesPageFiltersDto({required this.customers});

  final List<SalesPageFilterCustomerDto> customers;

  factory SalesPageFiltersDto.fromJson(Map<String, dynamic> json) =>
      _$SalesPageFiltersDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesPageFiltersDtoToJson(this);
}

@JsonSerializable()
class SalesPageRowDto {
  SalesPageRowDto({
    required this.saleId,
    required this.status,
    required this.soldWorldDay,
    required this.customerName,
    required this.totalMinor,
  });

  final String saleId;
  final String status;
  final int soldWorldDay;
  final String? customerName;
  final int totalMinor;

  factory SalesPageRowDto.fromJson(Map<String, dynamic> json) =>
      _$SalesPageRowDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesPageRowDtoToJson(this);
}

@JsonSerializable()
class SalesPageDto {
  SalesPageDto({
    required this.campaignId,
    required this.currencyCode,
    required this.filters,
    required this.sales,
  });

  final String campaignId;
  final String currencyCode;
  final SalesPageFiltersDto filters;
  final List<SalesPageRowDto> sales;

  factory SalesPageDto.fromJson(Map<String, dynamic> json) =>
      _$SalesPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesPageDtoToJson(this);
}

@JsonSerializable()
class SalePageFilterStorageLocationDto {
  SalePageFilterStorageLocationDto({
    required this.storageLocationId,
    required this.name,
  });

  final String storageLocationId;
  final String name;

  factory SalePageFilterStorageLocationDto.fromJson(Map<String, dynamic> json) =>
      _$SalePageFilterStorageLocationDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalePageFilterStorageLocationDtoToJson(this);
}

@JsonSerializable()
class SalePageFiltersDto {
  SalePageFiltersDto({
    required this.customers,
    required this.storageLocations,
  });

  final List<SalesPageFilterCustomerDto> customers;
  final List<SalePageFilterStorageLocationDto> storageLocations;

  factory SalePageFiltersDto.fromJson(Map<String, dynamic> json) =>
      _$SalePageFiltersDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalePageFiltersDtoToJson(this);
}

@JsonSerializable()
class SalesTotalsDto {
  SalesTotalsDto({
    required this.subtotalMinor,
    required this.discountTotalMinor,
    required this.taxTotalMinor,
    required this.totalMinor,
  });

  final int subtotalMinor;
  final int discountTotalMinor;
  final int taxTotalMinor;
  final int totalMinor;

  factory SalesTotalsDto.fromJson(Map<String, dynamic> json) =>
      _$SalesTotalsDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesTotalsDtoToJson(this);
}

@JsonSerializable()
class SalesLineDto {
  SalesLineDto({
    required this.saleLineId,
    required this.itemId,
    required this.quantity,
    required this.unitSoldPriceMinor,
    required this.unitTrueValueMinor,
    required this.discountMinor,
    required this.notes,
    required this.lineSubtotalMinor,
  });

  final String saleLineId;
  final String itemId;
  final double quantity;
  final int unitSoldPriceMinor;
  final int? unitTrueValueMinor;
  final int discountMinor;
  final String? notes;
  final int lineSubtotalMinor;

  factory SalesLineDto.fromJson(Map<String, dynamic> json) =>
      _$SalesLineDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesLineDtoToJson(this);
}

@JsonSerializable()
class SalesPaymentDto {
  SalesPaymentDto({
    required this.paymentId,
    required this.method,
    required this.amountMinor,
    required this.details,
  });

  final String paymentId;
  final String method;
  final int amountMinor;
  final Object? details;

  factory SalesPaymentDto.fromJson(Map<String, dynamic> json) =>
      _$SalesPaymentDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesPaymentDtoToJson(this);
}

@JsonSerializable()
class SalesDetailDto {
  SalesDetailDto({
    required this.saleId,
    required this.campaignId,
    required this.status,
    required this.soldWorldDay,
    required this.customerId,
    required this.storageLocationId,
    required this.notes,
    required this.totals,
    required this.lines,
    required this.payments,
  });

  final String saleId;
  final String campaignId;
  final String status;
  final int soldWorldDay;
  final String? customerId;
  final String storageLocationId;
  final String? notes;
  final SalesTotalsDto totals;
  final List<SalesLineDto> lines;
  final List<SalesPaymentDto> payments;

  factory SalesDetailDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDetailDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDetailDtoToJson(this);
}

@JsonSerializable()
class SalesDraftItemOptionDto {
  SalesDraftItemOptionDto({
    required this.itemId,
    required this.name,
    required this.baseValueMinor,
    required this.defaultListPriceMinor,
    required this.isArchived,
  });

  final String itemId;
  final String name;
  final int baseValueMinor;
  final int? defaultListPriceMinor;
  final bool isArchived;

  factory SalesDraftItemOptionDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftItemOptionDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftItemOptionDtoToJson(this);
}

@JsonSerializable()
class SalesDraftPageDto {
  SalesDraftPageDto({
    required this.campaignId,
    required this.currencyCode,
    required this.draft,
    required this.filters,
    required this.itemOptions,
  });

  final String campaignId;
  final String currencyCode;
  final SalesDetailDto draft;
  final SalePageFiltersDto filters;
  final List<SalesDraftItemOptionDto> itemOptions;

  factory SalesDraftPageDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftPageDtoToJson(this);
}

@JsonSerializable()
class SalesReceiptPageDto {
  SalesReceiptPageDto({
    required this.campaignId,
    required this.currencyCode,
    required this.sale,
    required this.filters,
  });

  final String campaignId;
  final String currencyCode;
  final SalesDetailDto sale;
  final SalePageFiltersDto filters;

  factory SalesReceiptPageDto.fromJson(Map<String, dynamic> json) =>
      _$SalesReceiptPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesReceiptPageDtoToJson(this);
}

@JsonSerializable()
class SalesDraftCreateRequestDto {
  SalesDraftCreateRequestDto({
    required this.campaignId,
    required this.soldWorldDay,
    required this.storageLocationId,
    required this.customerId,
    required this.notes,
  });

  final String campaignId;
  final int soldWorldDay;
  final String storageLocationId;
  final String? customerId;
  final String? notes;

  factory SalesDraftCreateRequestDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftCreateRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftCreateRequestDtoToJson(this);
}

@JsonSerializable()
class SalesDraftCreateResponseDto {
  SalesDraftCreateResponseDto({required this.draftId});

  final String draftId;

  factory SalesDraftCreateResponseDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftCreateResponseDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftCreateResponseDtoToJson(this);
}

@JsonSerializable()
class SalesDraftAddLineRequestDto {
  SalesDraftAddLineRequestDto({
    required this.campaignId,
    required this.draftId,
    required this.itemId,
    required this.quantity,
    required this.unitSoldPriceMinor,
    required this.unitTrueValueMinor,
    required this.discountMinor,
    required this.notes,
  });

  final String campaignId;
  final String draftId;
  final String itemId;
  final double quantity;
  final int? unitSoldPriceMinor;
  final int? unitTrueValueMinor;
  final int? discountMinor;
  final String? notes;

  factory SalesDraftAddLineRequestDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftAddLineRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftAddLineRequestDtoToJson(this);
}

@JsonSerializable()
class SalesDraftUpdateLineRequestDto {
  SalesDraftUpdateLineRequestDto({
    required this.campaignId,
    required this.draftId,
    required this.saleLineId,
    required this.quantity,
    required this.unitSoldPriceMinor,
    required this.unitTrueValueMinor,
    required this.discountMinor,
    required this.notes,
  });

  final String campaignId;
  final String draftId;
  final String saleLineId;
  final double quantity;
  final int unitSoldPriceMinor;
  final int? unitTrueValueMinor;
  final int discountMinor;
  final String? notes;

  factory SalesDraftUpdateLineRequestDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftUpdateLineRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftUpdateLineRequestDtoToJson(this);
}

@JsonSerializable()
class SalesDraftRemoveLineRequestDto {
  SalesDraftRemoveLineRequestDto({
    required this.campaignId,
    required this.draftId,
    required this.saleLineId,
  });

  final String campaignId;
  final String draftId;
  final String saleLineId;

  factory SalesDraftRemoveLineRequestDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftRemoveLineRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftRemoveLineRequestDtoToJson(this);
}

@JsonSerializable()
class SalesDraftCompleteRequestDto {
  SalesDraftCompleteRequestDto({
    required this.campaignId,
    required this.draftId,
  });

  final String campaignId;
  final String draftId;

  factory SalesDraftCompleteRequestDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftCompleteRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftCompleteRequestDtoToJson(this);
}

@JsonSerializable()
class SalesDraftMutationResponseDto {
  SalesDraftMutationResponseDto({
    required this.draftId,
    required this.updated,
  });

  final String draftId;
  final bool updated;

  factory SalesDraftMutationResponseDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftMutationResponseDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftMutationResponseDtoToJson(this);
}

@JsonSerializable()
class SalesDraftCompleteResponseDto {
  SalesDraftCompleteResponseDto({
    required this.saleId,
    required this.status,
  });

  final String saleId;
  final String status;

  factory SalesDraftCompleteResponseDto.fromJson(Map<String, dynamic> json) =>
      _$SalesDraftCompleteResponseDtoFromJson(json);
  Map<String, dynamic> toJson() => _$SalesDraftCompleteResponseDtoToJson(this);
}
