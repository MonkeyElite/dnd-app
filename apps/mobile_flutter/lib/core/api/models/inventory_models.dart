import 'package:json_annotation/json_annotation.dart';

part 'inventory_models.g.dart';

@JsonSerializable()
class InventoryPageFilterPlaceDto {
  InventoryPageFilterPlaceDto({
    required this.placeId,
    required this.name,
    required this.type,
  });

  final String placeId;
  final String name;
  final String type;

  factory InventoryPageFilterPlaceDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryPageFilterPlaceDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryPageFilterPlaceDtoToJson(this);
}

@JsonSerializable()
class InventoryPageFilterStorageLocationDto {
  InventoryPageFilterStorageLocationDto({
    required this.storageLocationId,
    required this.placeId,
    required this.name,
    required this.type,
    required this.code,
  });

  final String storageLocationId;
  final String? placeId;
  final String name;
  final String type;
  final String? code;

  factory InventoryPageFilterStorageLocationDto.fromJson(
    Map<String, dynamic> json,
  ) =>
      _$InventoryPageFilterStorageLocationDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryPageFilterStorageLocationDtoToJson(this);
}

@JsonSerializable()
class InventoryPageFiltersDto {
  InventoryPageFiltersDto({
    required this.places,
    required this.storageLocations,
  });

  final List<InventoryPageFilterPlaceDto> places;
  final List<InventoryPageFilterStorageLocationDto> storageLocations;

  factory InventoryPageFiltersDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryPageFiltersDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryPageFiltersDtoToJson(this);
}

@JsonSerializable()
class InventoryPageImageDto {
  InventoryPageImageDto({
    required this.assetId,
    required this.url,
  });

  final String? assetId;
  final String? url;

  factory InventoryPageImageDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryPageImageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryPageImageDtoToJson(this);
}

@JsonSerializable()
class InventoryPageRowDto {
  InventoryPageRowDto({
    required this.itemId,
    required this.itemName,
    required this.categoryName,
    required this.unitName,
    required this.image,
    required this.storageLocationId,
    required this.storageLocationName,
    required this.onHandQuantity,
  });

  final String itemId;
  final String itemName;
  final String categoryName;
  final String unitName;
  final InventoryPageImageDto image;
  final String storageLocationId;
  final String storageLocationName;
  final double onHandQuantity;

  factory InventoryPageRowDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryPageRowDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryPageRowDtoToJson(this);
}

@JsonSerializable()
class InventoryPageDto {
  InventoryPageDto({
    required this.campaignId,
    required this.currencyCode,
    required this.filters,
    required this.rows,
  });

  final String campaignId;
  final String currencyCode;
  final InventoryPageFiltersDto filters;
  final List<InventoryPageRowDto> rows;

  factory InventoryPageDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryPageDtoToJson(this);
}

@JsonSerializable()
class InventoryLocationsPageRowDto {
  InventoryLocationsPageRowDto({
    required this.storageLocationId,
    required this.placeId,
    required this.placeName,
    required this.name,
    required this.type,
    required this.code,
    required this.totalQuantity,
  });

  final String storageLocationId;
  final String? placeId;
  final String? placeName;
  final String name;
  final String type;
  final String? code;
  final double totalQuantity;

  factory InventoryLocationsPageRowDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryLocationsPageRowDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryLocationsPageRowDtoToJson(this);
}

@JsonSerializable()
class InventoryLocationsPageDto {
  InventoryLocationsPageDto({
    required this.campaignId,
    required this.locations,
  });

  final String campaignId;
  final List<InventoryLocationsPageRowDto> locations;

  factory InventoryLocationsPageDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryLocationsPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryLocationsPageDtoToJson(this);
}

@JsonSerializable()
class InventoryLocationDetailLotPageRowDto {
  InventoryLocationDetailLotPageRowDto({
    required this.lotId,
    required this.itemId,
    required this.itemName,
    required this.quantityOnHand,
    required this.unitCostMinor,
    required this.acquiredWorldDay,
    required this.source,
    required this.notes,
  });

  final String lotId;
  final String itemId;
  final String itemName;
  final double quantityOnHand;
  final int unitCostMinor;
  final int acquiredWorldDay;
  final String? source;
  final String? notes;

  factory InventoryLocationDetailLotPageRowDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryLocationDetailLotPageRowDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryLocationDetailLotPageRowDtoToJson(this);
}

@JsonSerializable()
class InventoryLocationDetailAdjustmentPageRowDto {
  InventoryLocationDetailAdjustmentPageRowDto({
    required this.adjustmentId,
    required this.itemId,
    required this.itemName,
    required this.lotId,
    required this.deltaQuantity,
    required this.reason,
    required this.worldDay,
    required this.notes,
    required this.referenceType,
    required this.referenceId,
    required this.createdAt,
  });

  final String adjustmentId;
  final String itemId;
  final String itemName;
  final String? lotId;
  final double deltaQuantity;
  final String reason;
  final int worldDay;
  final String? notes;
  final String? referenceType;
  final String? referenceId;
  final DateTime createdAt;

  factory InventoryLocationDetailAdjustmentPageRowDto.fromJson(
    Map<String, dynamic> json,
  ) =>
      _$InventoryLocationDetailAdjustmentPageRowDtoFromJson(json);
  Map<String, dynamic> toJson() =>
      _$InventoryLocationDetailAdjustmentPageRowDtoToJson(this);
}

@JsonSerializable()
class InventoryLocationDetailPageDto {
  InventoryLocationDetailPageDto({
    required this.campaignId,
    required this.storageLocationId,
    required this.storageLocationName,
    required this.storageLocationCode,
    required this.storageLocationType,
    required this.placeId,
    required this.placeName,
    required this.currencyCode,
    required this.lots,
    required this.adjustments,
  });

  final String campaignId;
  final String storageLocationId;
  final String storageLocationName;
  final String? storageLocationCode;
  final String storageLocationType;
  final String? placeId;
  final String? placeName;
  final String currencyCode;
  final List<InventoryLocationDetailLotPageRowDto> lots;
  final List<InventoryLocationDetailAdjustmentPageRowDto> adjustments;

  factory InventoryLocationDetailPageDto.fromJson(Map<String, dynamic> json) =>
      _$InventoryLocationDetailPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$InventoryLocationDetailPageDtoToJson(this);
}
