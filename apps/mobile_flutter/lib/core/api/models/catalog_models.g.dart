// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'catalog_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CatalogPageFilterCategoryDto _$CatalogPageFilterCategoryDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageFilterCategoryDto(
  categoryId: json['categoryId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$CatalogPageFilterCategoryDtoToJson(
  CatalogPageFilterCategoryDto instance,
) => <String, dynamic>{
  'categoryId': instance.categoryId,
  'name': instance.name,
};

CatalogPageFilterUnitDto _$CatalogPageFilterUnitDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageFilterUnitDto(
  unitId: json['unitId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$CatalogPageFilterUnitDtoToJson(
  CatalogPageFilterUnitDto instance,
) => <String, dynamic>{'unitId': instance.unitId, 'name': instance.name};

CatalogPageFilterTagDto _$CatalogPageFilterTagDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageFilterTagDto(
  tagId: json['tagId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$CatalogPageFilterTagDtoToJson(
  CatalogPageFilterTagDto instance,
) => <String, dynamic>{'tagId': instance.tagId, 'name': instance.name};

CatalogPageFiltersDto _$CatalogPageFiltersDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageFiltersDto(
  categories: (json['categories'] as List<dynamic>)
      .map(
        (e) => CatalogPageFilterCategoryDto.fromJson(e as Map<String, dynamic>),
      )
      .toList(),
  units: (json['units'] as List<dynamic>)
      .map((e) => CatalogPageFilterUnitDto.fromJson(e as Map<String, dynamic>))
      .toList(),
  tags: (json['tags'] as List<dynamic>)
      .map((e) => CatalogPageFilterTagDto.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$CatalogPageFiltersDtoToJson(
  CatalogPageFiltersDto instance,
) => <String, dynamic>{
  'categories': instance.categories,
  'units': instance.units,
  'tags': instance.tags,
};

CatalogPageItemCategoryDto _$CatalogPageItemCategoryDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageItemCategoryDto(
  categoryId: json['categoryId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$CatalogPageItemCategoryDtoToJson(
  CatalogPageItemCategoryDto instance,
) => <String, dynamic>{
  'categoryId': instance.categoryId,
  'name': instance.name,
};

CatalogPageItemUnitDto _$CatalogPageItemUnitDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageItemUnitDto(
  unitId: json['unitId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$CatalogPageItemUnitDtoToJson(
  CatalogPageItemUnitDto instance,
) => <String, dynamic>{'unitId': instance.unitId, 'name': instance.name};

CatalogPageItemImageDto _$CatalogPageItemImageDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageItemImageDto(
  assetId: json['assetId'] as String?,
  url: json['url'] as String?,
);

Map<String, dynamic> _$CatalogPageItemImageDtoToJson(
  CatalogPageItemImageDto instance,
) => <String, dynamic>{'assetId': instance.assetId, 'url': instance.url};

CatalogPageItemTagDto _$CatalogPageItemTagDtoFromJson(
  Map<String, dynamic> json,
) => CatalogPageItemTagDto(
  tagId: json['tagId'] as String,
  name: json['name'] as String,
);

Map<String, dynamic> _$CatalogPageItemTagDtoToJson(
  CatalogPageItemTagDto instance,
) => <String, dynamic>{'tagId': instance.tagId, 'name': instance.name};

CatalogPageItemDto _$CatalogPageItemDtoFromJson(Map<String, dynamic> json) =>
    CatalogPageItemDto(
      itemId: json['itemId'] as String,
      name: json['name'] as String,
      description: json['description'] as String?,
      category: CatalogPageItemCategoryDto.fromJson(
        json['category'] as Map<String, dynamic>,
      ),
      unit: CatalogPageItemUnitDto.fromJson(
        json['unit'] as Map<String, dynamic>,
      ),
      baseValueMinor: (json['baseValueMinor'] as num).toInt(),
      defaultListPriceMinor: (json['defaultListPriceMinor'] as num?)?.toInt(),
      weight: (json['weight'] as num?)?.toDouble(),
      image: CatalogPageItemImageDto.fromJson(
        json['image'] as Map<String, dynamic>,
      ),
      tags: (json['tags'] as List<dynamic>)
          .map((e) => CatalogPageItemTagDto.fromJson(e as Map<String, dynamic>))
          .toList(),
      isArchived: json['isArchived'] as bool,
    );

Map<String, dynamic> _$CatalogPageItemDtoToJson(CatalogPageItemDto instance) =>
    <String, dynamic>{
      'itemId': instance.itemId,
      'name': instance.name,
      'description': instance.description,
      'category': instance.category,
      'unit': instance.unit,
      'baseValueMinor': instance.baseValueMinor,
      'defaultListPriceMinor': instance.defaultListPriceMinor,
      'weight': instance.weight,
      'image': instance.image,
      'tags': instance.tags,
      'isArchived': instance.isArchived,
    };

CatalogPageDto _$CatalogPageDtoFromJson(Map<String, dynamic> json) =>
    CatalogPageDto(
      campaignId: json['campaignId'] as String,
      currencyCode: json['currencyCode'] as String,
      filters: CatalogPageFiltersDto.fromJson(
        json['filters'] as Map<String, dynamic>,
      ),
      items: (json['items'] as List<dynamic>)
          .map((e) => CatalogPageItemDto.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$CatalogPageDtoToJson(CatalogPageDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'currencyCode': instance.currencyCode,
      'filters': instance.filters,
      'items': instance.items,
    };

CatalogItemPageDto _$CatalogItemPageDtoFromJson(Map<String, dynamic> json) =>
    CatalogItemPageDto(
      campaignId: json['campaignId'] as String,
      currencyCode: json['currencyCode'] as String,
      item: CatalogPageItemDto.fromJson(json['item'] as Map<String, dynamic>),
    );

Map<String, dynamic> _$CatalogItemPageDtoToJson(CatalogItemPageDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'currencyCode': instance.currencyCode,
      'item': instance.item,
    };
