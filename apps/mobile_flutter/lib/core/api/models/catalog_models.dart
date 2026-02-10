import 'package:json_annotation/json_annotation.dart';

part 'catalog_models.g.dart';

@JsonSerializable()
class CatalogPageFilterCategoryDto {
  CatalogPageFilterCategoryDto({
    required this.categoryId,
    required this.name,
  });

  final String categoryId;
  final String name;

  factory CatalogPageFilterCategoryDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageFilterCategoryDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageFilterCategoryDtoToJson(this);
}

@JsonSerializable()
class CatalogPageFilterUnitDto {
  CatalogPageFilterUnitDto({
    required this.unitId,
    required this.name,
  });

  final String unitId;
  final String name;

  factory CatalogPageFilterUnitDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageFilterUnitDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageFilterUnitDtoToJson(this);
}

@JsonSerializable()
class CatalogPageFilterTagDto {
  CatalogPageFilterTagDto({
    required this.tagId,
    required this.name,
  });

  final String tagId;
  final String name;

  factory CatalogPageFilterTagDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageFilterTagDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageFilterTagDtoToJson(this);
}

@JsonSerializable()
class CatalogPageFiltersDto {
  CatalogPageFiltersDto({
    required this.categories,
    required this.units,
    required this.tags,
  });

  final List<CatalogPageFilterCategoryDto> categories;
  final List<CatalogPageFilterUnitDto> units;
  final List<CatalogPageFilterTagDto> tags;

  factory CatalogPageFiltersDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageFiltersDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageFiltersDtoToJson(this);
}

@JsonSerializable()
class CatalogPageItemCategoryDto {
  CatalogPageItemCategoryDto({
    required this.categoryId,
    required this.name,
  });

  final String categoryId;
  final String name;

  factory CatalogPageItemCategoryDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageItemCategoryDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageItemCategoryDtoToJson(this);
}

@JsonSerializable()
class CatalogPageItemUnitDto {
  CatalogPageItemUnitDto({
    required this.unitId,
    required this.name,
  });

  final String unitId;
  final String name;

  factory CatalogPageItemUnitDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageItemUnitDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageItemUnitDtoToJson(this);
}

@JsonSerializable()
class CatalogPageItemImageDto {
  CatalogPageItemImageDto({
    required this.assetId,
    required this.url,
  });

  final String? assetId;
  final String? url;

  factory CatalogPageItemImageDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageItemImageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageItemImageDtoToJson(this);
}

@JsonSerializable()
class CatalogPageItemTagDto {
  CatalogPageItemTagDto({
    required this.tagId,
    required this.name,
  });

  final String tagId;
  final String name;

  factory CatalogPageItemTagDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageItemTagDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageItemTagDtoToJson(this);
}

@JsonSerializable()
class CatalogPageItemDto {
  CatalogPageItemDto({
    required this.itemId,
    required this.name,
    required this.description,
    required this.category,
    required this.unit,
    required this.baseValueMinor,
    required this.defaultListPriceMinor,
    required this.weight,
    required this.image,
    required this.tags,
    required this.isArchived,
  });

  final String itemId;
  final String name;
  final String? description;
  final CatalogPageItemCategoryDto category;
  final CatalogPageItemUnitDto unit;
  final int baseValueMinor;
  final int? defaultListPriceMinor;
  final double? weight;
  final CatalogPageItemImageDto image;
  final List<CatalogPageItemTagDto> tags;
  final bool isArchived;

  factory CatalogPageItemDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageItemDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageItemDtoToJson(this);
}

@JsonSerializable()
class CatalogPageDto {
  CatalogPageDto({
    required this.campaignId,
    required this.currencyCode,
    required this.filters,
    required this.items,
  });

  final String campaignId;
  final String currencyCode;
  final CatalogPageFiltersDto filters;
  final List<CatalogPageItemDto> items;

  factory CatalogPageDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogPageDtoToJson(this);
}

@JsonSerializable()
class CatalogItemPageDto {
  CatalogItemPageDto({
    required this.campaignId,
    required this.currencyCode,
    required this.item,
  });

  final String campaignId;
  final String currencyCode;
  final CatalogPageItemDto item;

  factory CatalogItemPageDto.fromJson(Map<String, dynamic> json) =>
      _$CatalogItemPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CatalogItemPageDtoToJson(this);
}
