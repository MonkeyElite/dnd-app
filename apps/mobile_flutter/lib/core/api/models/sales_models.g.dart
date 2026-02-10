// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'sales_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

SalesPageFilterCustomerDto _$SalesPageFilterCustomerDtoFromJson(
  Map<String, dynamic> json,
) => SalesPageFilterCustomerDto(
  customerId: json['customerId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$SalesPageFilterCustomerDtoToJson(
  SalesPageFilterCustomerDto instance,
) => <String, dynamic>{
  'customerId': instance.customerId,
  'name': instance.name,
};

SalesPageFiltersDto _$SalesPageFiltersDtoFromJson(Map<String, dynamic> json) =>
    SalesPageFiltersDto(
      customers: (json['customers'] as List<dynamic>)
          .map(
            (e) =>
                SalesPageFilterCustomerDto.fromJson(e as Map<String, dynamic>),
          )
          .toList(),
    );

Map<String, dynamic> _$SalesPageFiltersDtoToJson(
  SalesPageFiltersDto instance,
) => <String, dynamic>{'customers': instance.customers};

SalesPageRowDto _$SalesPageRowDtoFromJson(Map<String, dynamic> json) =>
    SalesPageRowDto(
      saleId: json['saleId'] as String,
      status: json['status'] as String,
      soldWorldDay: (json['soldWorldDay'] as num).toInt(),
      customerName: json['customerName'] as String?,
      totalMinor: (json['totalMinor'] as num).toInt(),
    );

Map<String, dynamic> _$SalesPageRowDtoToJson(SalesPageRowDto instance) =>
    <String, dynamic>{
      'saleId': instance.saleId,
      'status': instance.status,
      'soldWorldDay': instance.soldWorldDay,
      'customerName': instance.customerName,
      'totalMinor': instance.totalMinor,
    };

SalesPageDto _$SalesPageDtoFromJson(Map<String, dynamic> json) => SalesPageDto(
  campaignId: json['campaignId'] as String,
  currencyCode: json['currencyCode'] as String,
  filters: SalesPageFiltersDto.fromJson(
    json['filters'] as Map<String, dynamic>,
  ),
  sales: (json['sales'] as List<dynamic>)
      .map((e) => SalesPageRowDto.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$SalesPageDtoToJson(SalesPageDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'currencyCode': instance.currencyCode,
      'filters': instance.filters,
      'sales': instance.sales,
    };

SalePageFilterStorageLocationDto _$SalePageFilterStorageLocationDtoFromJson(
  Map<String, dynamic> json,
) => SalePageFilterStorageLocationDto(
  storageLocationId: json['storageLocationId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$SalePageFilterStorageLocationDtoToJson(
  SalePageFilterStorageLocationDto instance,
) => <String, dynamic>{
  'storageLocationId': instance.storageLocationId,
  'name': instance.name,
};

SalePageFiltersDto _$SalePageFiltersDtoFromJson(Map<String, dynamic> json) =>
    SalePageFiltersDto(
      customers: (json['customers'] as List<dynamic>)
          .map(
            (e) =>
                SalesPageFilterCustomerDto.fromJson(e as Map<String, dynamic>),
          )
          .toList(),
      storageLocations: (json['storageLocations'] as List<dynamic>)
          .map(
            (e) => SalePageFilterStorageLocationDto.fromJson(
              e as Map<String, dynamic>,
            ),
          )
          .toList(),
    );

Map<String, dynamic> _$SalePageFiltersDtoToJson(SalePageFiltersDto instance) =>
    <String, dynamic>{
      'customers': instance.customers,
      'storageLocations': instance.storageLocations,
    };

SalesTotalsDto _$SalesTotalsDtoFromJson(Map<String, dynamic> json) =>
    SalesTotalsDto(
      subtotalMinor: (json['subtotalMinor'] as num).toInt(),
      discountTotalMinor: (json['discountTotalMinor'] as num).toInt(),
      taxTotalMinor: (json['taxTotalMinor'] as num).toInt(),
      totalMinor: (json['totalMinor'] as num).toInt(),
    );

Map<String, dynamic> _$SalesTotalsDtoToJson(SalesTotalsDto instance) =>
    <String, dynamic>{
      'subtotalMinor': instance.subtotalMinor,
      'discountTotalMinor': instance.discountTotalMinor,
      'taxTotalMinor': instance.taxTotalMinor,
      'totalMinor': instance.totalMinor,
    };

SalesLineDto _$SalesLineDtoFromJson(Map<String, dynamic> json) => SalesLineDto(
  saleLineId: json['saleLineId'] as String,
  itemId: json['itemId'] as String,
  quantity: (json['quantity'] as num).toDouble(),
  unitSoldPriceMinor: (json['unitSoldPriceMinor'] as num).toInt(),
  unitTrueValueMinor: (json['unitTrueValueMinor'] as num?)?.toInt(),
  discountMinor: (json['discountMinor'] as num).toInt(),
  notes: json['notes'] as String?,
  lineSubtotalMinor: (json['lineSubtotalMinor'] as num).toInt(),
);

Map<String, dynamic> _$SalesLineDtoToJson(SalesLineDto instance) =>
    <String, dynamic>{
      'saleLineId': instance.saleLineId,
      'itemId': instance.itemId,
      'quantity': instance.quantity,
      'unitSoldPriceMinor': instance.unitSoldPriceMinor,
      'unitTrueValueMinor': instance.unitTrueValueMinor,
      'discountMinor': instance.discountMinor,
      'notes': instance.notes,
      'lineSubtotalMinor': instance.lineSubtotalMinor,
    };

SalesPaymentDto _$SalesPaymentDtoFromJson(Map<String, dynamic> json) =>
    SalesPaymentDto(
      paymentId: json['paymentId'] as String,
      method: json['method'] as String,
      amountMinor: (json['amountMinor'] as num).toInt(),
      details: json['details'],
    );

Map<String, dynamic> _$SalesPaymentDtoToJson(SalesPaymentDto instance) =>
    <String, dynamic>{
      'paymentId': instance.paymentId,
      'method': instance.method,
      'amountMinor': instance.amountMinor,
      'details': instance.details,
    };

SalesDetailDto _$SalesDetailDtoFromJson(Map<String, dynamic> json) =>
    SalesDetailDto(
      saleId: json['saleId'] as String,
      campaignId: json['campaignId'] as String,
      status: json['status'] as String,
      soldWorldDay: (json['soldWorldDay'] as num).toInt(),
      customerId: json['customerId'] as String?,
      storageLocationId: json['storageLocationId'] as String,
      notes: json['notes'] as String?,
      totals: SalesTotalsDto.fromJson(json['totals'] as Map<String, dynamic>),
      lines: (json['lines'] as List<dynamic>)
          .map((e) => SalesLineDto.fromJson(e as Map<String, dynamic>))
          .toList(),
      payments: (json['payments'] as List<dynamic>)
          .map((e) => SalesPaymentDto.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$SalesDetailDtoToJson(SalesDetailDto instance) =>
    <String, dynamic>{
      'saleId': instance.saleId,
      'campaignId': instance.campaignId,
      'status': instance.status,
      'soldWorldDay': instance.soldWorldDay,
      'customerId': instance.customerId,
      'storageLocationId': instance.storageLocationId,
      'notes': instance.notes,
      'totals': instance.totals,
      'lines': instance.lines,
      'payments': instance.payments,
    };

SalesDraftItemOptionDto _$SalesDraftItemOptionDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftItemOptionDto(
  itemId: json['itemId'] as String,
  name: json['name'] as String,
  baseValueMinor: (json['baseValueMinor'] as num).toInt(),
  defaultListPriceMinor: (json['defaultListPriceMinor'] as num?)?.toInt(),
  isArchived: json['isArchived'] as bool,
);

Map<String, dynamic> _$SalesDraftItemOptionDtoToJson(
  SalesDraftItemOptionDto instance,
) => <String, dynamic>{
  'itemId': instance.itemId,
  'name': instance.name,
  'baseValueMinor': instance.baseValueMinor,
  'defaultListPriceMinor': instance.defaultListPriceMinor,
  'isArchived': instance.isArchived,
};

SalesDraftPageDto _$SalesDraftPageDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftPageDto(
  campaignId: json['campaignId'] as String,
  currencyCode: json['currencyCode'] as String,
  draft: SalesDetailDto.fromJson(json['draft'] as Map<String, dynamic>),
  filters: SalePageFiltersDto.fromJson(json['filters'] as Map<String, dynamic>),
  itemOptions: (json['itemOptions'] as List<dynamic>)
      .map((e) => SalesDraftItemOptionDto.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$SalesDraftPageDtoToJson(SalesDraftPageDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'currencyCode': instance.currencyCode,
      'draft': instance.draft,
      'filters': instance.filters,
      'itemOptions': instance.itemOptions,
    };

SalesReceiptPageDto _$SalesReceiptPageDtoFromJson(Map<String, dynamic> json) =>
    SalesReceiptPageDto(
      campaignId: json['campaignId'] as String,
      currencyCode: json['currencyCode'] as String,
      sale: SalesDetailDto.fromJson(json['sale'] as Map<String, dynamic>),
      filters: SalePageFiltersDto.fromJson(
        json['filters'] as Map<String, dynamic>,
      ),
    );

Map<String, dynamic> _$SalesReceiptPageDtoToJson(
  SalesReceiptPageDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'currencyCode': instance.currencyCode,
  'sale': instance.sale,
  'filters': instance.filters,
};

SalesDraftCreateRequestDto _$SalesDraftCreateRequestDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftCreateRequestDto(
  campaignId: json['campaignId'] as String,
  soldWorldDay: (json['soldWorldDay'] as num).toInt(),
  storageLocationId: json['storageLocationId'] as String,
  customerId: json['customerId'] as String?,
  notes: json['notes'] as String?,
);

Map<String, dynamic> _$SalesDraftCreateRequestDtoToJson(
  SalesDraftCreateRequestDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'soldWorldDay': instance.soldWorldDay,
  'storageLocationId': instance.storageLocationId,
  'customerId': instance.customerId,
  'notes': instance.notes,
};

SalesDraftCreateResponseDto _$SalesDraftCreateResponseDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftCreateResponseDto(draftId: json['draftId'] as String);

Map<String, dynamic> _$SalesDraftCreateResponseDtoToJson(
  SalesDraftCreateResponseDto instance,
) => <String, dynamic>{'draftId': instance.draftId};

SalesDraftAddLineRequestDto _$SalesDraftAddLineRequestDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftAddLineRequestDto(
  campaignId: json['campaignId'] as String,
  draftId: json['draftId'] as String,
  itemId: json['itemId'] as String,
  quantity: (json['quantity'] as num).toDouble(),
  unitSoldPriceMinor: (json['unitSoldPriceMinor'] as num?)?.toInt(),
  unitTrueValueMinor: (json['unitTrueValueMinor'] as num?)?.toInt(),
  discountMinor: (json['discountMinor'] as num?)?.toInt(),
  notes: json['notes'] as String?,
);

Map<String, dynamic> _$SalesDraftAddLineRequestDtoToJson(
  SalesDraftAddLineRequestDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'draftId': instance.draftId,
  'itemId': instance.itemId,
  'quantity': instance.quantity,
  'unitSoldPriceMinor': instance.unitSoldPriceMinor,
  'unitTrueValueMinor': instance.unitTrueValueMinor,
  'discountMinor': instance.discountMinor,
  'notes': instance.notes,
};

SalesDraftUpdateLineRequestDto _$SalesDraftUpdateLineRequestDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftUpdateLineRequestDto(
  campaignId: json['campaignId'] as String,
  draftId: json['draftId'] as String,
  saleLineId: json['saleLineId'] as String,
  quantity: (json['quantity'] as num).toDouble(),
  unitSoldPriceMinor: (json['unitSoldPriceMinor'] as num).toInt(),
  unitTrueValueMinor: (json['unitTrueValueMinor'] as num?)?.toInt(),
  discountMinor: (json['discountMinor'] as num).toInt(),
  notes: json['notes'] as String?,
);

Map<String, dynamic> _$SalesDraftUpdateLineRequestDtoToJson(
  SalesDraftUpdateLineRequestDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'draftId': instance.draftId,
  'saleLineId': instance.saleLineId,
  'quantity': instance.quantity,
  'unitSoldPriceMinor': instance.unitSoldPriceMinor,
  'unitTrueValueMinor': instance.unitTrueValueMinor,
  'discountMinor': instance.discountMinor,
  'notes': instance.notes,
};

SalesDraftRemoveLineRequestDto _$SalesDraftRemoveLineRequestDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftRemoveLineRequestDto(
  campaignId: json['campaignId'] as String,
  draftId: json['draftId'] as String,
  saleLineId: json['saleLineId'] as String,
);

Map<String, dynamic> _$SalesDraftRemoveLineRequestDtoToJson(
  SalesDraftRemoveLineRequestDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'draftId': instance.draftId,
  'saleLineId': instance.saleLineId,
};

SalesDraftCompleteRequestDto _$SalesDraftCompleteRequestDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftCompleteRequestDto(
  campaignId: json['campaignId'] as String,
  draftId: json['draftId'] as String,
);

Map<String, dynamic> _$SalesDraftCompleteRequestDtoToJson(
  SalesDraftCompleteRequestDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'draftId': instance.draftId,
};

SalesDraftMutationResponseDto _$SalesDraftMutationResponseDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftMutationResponseDto(
  draftId: json['draftId'] as String,
  updated: json['updated'] as bool,
);

Map<String, dynamic> _$SalesDraftMutationResponseDtoToJson(
  SalesDraftMutationResponseDto instance,
) => <String, dynamic>{
  'draftId': instance.draftId,
  'updated': instance.updated,
};

SalesDraftCompleteResponseDto _$SalesDraftCompleteResponseDtoFromJson(
  Map<String, dynamic> json,
) => SalesDraftCompleteResponseDto(
  saleId: json['saleId'] as String,
  status: json['status'] as String,
);

Map<String, dynamic> _$SalesDraftCompleteResponseDtoToJson(
  SalesDraftCompleteResponseDto instance,
) => <String, dynamic>{'saleId': instance.saleId, 'status': instance.status};
