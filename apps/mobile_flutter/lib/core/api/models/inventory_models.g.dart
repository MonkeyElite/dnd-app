// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'inventory_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

InventoryPageFilterPlaceDto _$InventoryPageFilterPlaceDtoFromJson(
  Map<String, dynamic> json,
) => InventoryPageFilterPlaceDto(
  placeId: json['placeId'] as String,
  name: json['name'] as String,
  type: json['type'] as String,
);

Map<String, dynamic> _$InventoryPageFilterPlaceDtoToJson(
  InventoryPageFilterPlaceDto instance,
) => <String, dynamic>{
  'placeId': instance.placeId,
  'name': instance.name,
  'type': instance.type,
};

InventoryPageFilterStorageLocationDto
_$InventoryPageFilterStorageLocationDtoFromJson(Map<String, dynamic> json) =>
    InventoryPageFilterStorageLocationDto(
      storageLocationId: json['storageLocationId'] as String,
      placeId: json['placeId'] as String?,
      name: json['name'] as String,
      type: json['type'] as String,
      code: json['code'] as String?,
    );

Map<String, dynamic> _$InventoryPageFilterStorageLocationDtoToJson(
  InventoryPageFilterStorageLocationDto instance,
) => <String, dynamic>{
  'storageLocationId': instance.storageLocationId,
  'placeId': instance.placeId,
  'name': instance.name,
  'type': instance.type,
  'code': instance.code,
};

InventoryPageFiltersDto _$InventoryPageFiltersDtoFromJson(
  Map<String, dynamic> json,
) => InventoryPageFiltersDto(
  places: (json['places'] as List<dynamic>)
      .map(
        (e) => InventoryPageFilterPlaceDto.fromJson(e as Map<String, dynamic>),
      )
      .toList(),
  storageLocations: (json['storageLocations'] as List<dynamic>)
      .map(
        (e) => InventoryPageFilterStorageLocationDto.fromJson(
          e as Map<String, dynamic>,
        ),
      )
      .toList(),
);

Map<String, dynamic> _$InventoryPageFiltersDtoToJson(
  InventoryPageFiltersDto instance,
) => <String, dynamic>{
  'places': instance.places,
  'storageLocations': instance.storageLocations,
};

InventoryPageImageDto _$InventoryPageImageDtoFromJson(
  Map<String, dynamic> json,
) => InventoryPageImageDto(
  assetId: json['assetId'] as String?,
  url: json['url'] as String?,
);

Map<String, dynamic> _$InventoryPageImageDtoToJson(
  InventoryPageImageDto instance,
) => <String, dynamic>{'assetId': instance.assetId, 'url': instance.url};

InventoryPageRowDto _$InventoryPageRowDtoFromJson(Map<String, dynamic> json) =>
    InventoryPageRowDto(
      itemId: json['itemId'] as String,
      itemName: json['itemName'] as String,
      categoryName: json['categoryName'] as String,
      unitName: json['unitName'] as String,
      image: InventoryPageImageDto.fromJson(
        json['image'] as Map<String, dynamic>,
      ),
      storageLocationId: json['storageLocationId'] as String,
      storageLocationName: json['storageLocationName'] as String,
      onHandQuantity: (json['onHandQuantity'] as num).toDouble(),
    );

Map<String, dynamic> _$InventoryPageRowDtoToJson(
  InventoryPageRowDto instance,
) => <String, dynamic>{
  'itemId': instance.itemId,
  'itemName': instance.itemName,
  'categoryName': instance.categoryName,
  'unitName': instance.unitName,
  'image': instance.image,
  'storageLocationId': instance.storageLocationId,
  'storageLocationName': instance.storageLocationName,
  'onHandQuantity': instance.onHandQuantity,
};

InventoryPageDto _$InventoryPageDtoFromJson(Map<String, dynamic> json) =>
    InventoryPageDto(
      campaignId: json['campaignId'] as String,
      currencyCode: json['currencyCode'] as String,
      filters: InventoryPageFiltersDto.fromJson(
        json['filters'] as Map<String, dynamic>,
      ),
      rows: (json['rows'] as List<dynamic>)
          .map((e) => InventoryPageRowDto.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$InventoryPageDtoToJson(InventoryPageDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'currencyCode': instance.currencyCode,
      'filters': instance.filters,
      'rows': instance.rows,
    };

InventoryLocationsPageRowDto _$InventoryLocationsPageRowDtoFromJson(
  Map<String, dynamic> json,
) => InventoryLocationsPageRowDto(
  storageLocationId: json['storageLocationId'] as String,
  placeId: json['placeId'] as String?,
  placeName: json['placeName'] as String?,
  name: json['name'] as String,
  type: json['type'] as String,
  code: json['code'] as String?,
  totalQuantity: (json['totalQuantity'] as num).toDouble(),
);

Map<String, dynamic> _$InventoryLocationsPageRowDtoToJson(
  InventoryLocationsPageRowDto instance,
) => <String, dynamic>{
  'storageLocationId': instance.storageLocationId,
  'placeId': instance.placeId,
  'placeName': instance.placeName,
  'name': instance.name,
  'type': instance.type,
  'code': instance.code,
  'totalQuantity': instance.totalQuantity,
};

InventoryLocationsPageDto _$InventoryLocationsPageDtoFromJson(
  Map<String, dynamic> json,
) => InventoryLocationsPageDto(
  campaignId: json['campaignId'] as String,
  locations: (json['locations'] as List<dynamic>)
      .map(
        (e) => InventoryLocationsPageRowDto.fromJson(e as Map<String, dynamic>),
      )
      .toList(),
);

Map<String, dynamic> _$InventoryLocationsPageDtoToJson(
  InventoryLocationsPageDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'locations': instance.locations,
};

InventoryLocationDetailLotPageRowDto
_$InventoryLocationDetailLotPageRowDtoFromJson(Map<String, dynamic> json) =>
    InventoryLocationDetailLotPageRowDto(
      lotId: json['lotId'] as String,
      itemId: json['itemId'] as String,
      itemName: json['itemName'] as String,
      quantityOnHand: (json['quantityOnHand'] as num).toDouble(),
      unitCostMinor: (json['unitCostMinor'] as num).toInt(),
      acquiredWorldDay: (json['acquiredWorldDay'] as num).toInt(),
      source: json['source'] as String?,
      notes: json['notes'] as String?,
    );

Map<String, dynamic> _$InventoryLocationDetailLotPageRowDtoToJson(
  InventoryLocationDetailLotPageRowDto instance,
) => <String, dynamic>{
  'lotId': instance.lotId,
  'itemId': instance.itemId,
  'itemName': instance.itemName,
  'quantityOnHand': instance.quantityOnHand,
  'unitCostMinor': instance.unitCostMinor,
  'acquiredWorldDay': instance.acquiredWorldDay,
  'source': instance.source,
  'notes': instance.notes,
};

InventoryLocationDetailAdjustmentPageRowDto
_$InventoryLocationDetailAdjustmentPageRowDtoFromJson(
  Map<String, dynamic> json,
) => InventoryLocationDetailAdjustmentPageRowDto(
  adjustmentId: json['adjustmentId'] as String,
  itemId: json['itemId'] as String,
  itemName: json['itemName'] as String,
  lotId: json['lotId'] as String?,
  deltaQuantity: (json['deltaQuantity'] as num).toDouble(),
  reason: json['reason'] as String,
  worldDay: (json['worldDay'] as num).toInt(),
  notes: json['notes'] as String?,
  referenceType: json['referenceType'] as String?,
  referenceId: json['referenceId'] as String?,
  createdAt: DateTime.parse(json['createdAt'] as String),
);

Map<String, dynamic> _$InventoryLocationDetailAdjustmentPageRowDtoToJson(
  InventoryLocationDetailAdjustmentPageRowDto instance,
) => <String, dynamic>{
  'adjustmentId': instance.adjustmentId,
  'itemId': instance.itemId,
  'itemName': instance.itemName,
  'lotId': instance.lotId,
  'deltaQuantity': instance.deltaQuantity,
  'reason': instance.reason,
  'worldDay': instance.worldDay,
  'notes': instance.notes,
  'referenceType': instance.referenceType,
  'referenceId': instance.referenceId,
  'createdAt': instance.createdAt.toIso8601String(),
};

InventoryLocationDetailPageDto _$InventoryLocationDetailPageDtoFromJson(
  Map<String, dynamic> json,
) => InventoryLocationDetailPageDto(
  campaignId: json['campaignId'] as String,
  storageLocationId: json['storageLocationId'] as String,
  storageLocationName: json['storageLocationName'] as String,
  storageLocationCode: json['storageLocationCode'] as String?,
  storageLocationType: json['storageLocationType'] as String,
  placeId: json['placeId'] as String?,
  placeName: json['placeName'] as String?,
  currencyCode: json['currencyCode'] as String,
  lots: (json['lots'] as List<dynamic>)
      .map(
        (e) => InventoryLocationDetailLotPageRowDto.fromJson(
          e as Map<String, dynamic>,
        ),
      )
      .toList(),
  adjustments: (json['adjustments'] as List<dynamic>)
      .map(
        (e) => InventoryLocationDetailAdjustmentPageRowDto.fromJson(
          e as Map<String, dynamic>,
        ),
      )
      .toList(),
);

Map<String, dynamic> _$InventoryLocationDetailPageDtoToJson(
  InventoryLocationDetailPageDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'storageLocationId': instance.storageLocationId,
  'storageLocationName': instance.storageLocationName,
  'storageLocationCode': instance.storageLocationCode,
  'storageLocationType': instance.storageLocationType,
  'placeId': instance.placeId,
  'placeName': instance.placeName,
  'currencyCode': instance.currencyCode,
  'lots': instance.lots,
  'adjustments': instance.adjustments,
};
