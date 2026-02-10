import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/catalog_models.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class CatalogPageArgs {
  const CatalogPageArgs({
    required this.campaignId,
    this.search,
    this.categoryId,
  });

  final String campaignId;
  final String? search;
  final String? categoryId;

  @override
  bool operator ==(Object other) {
    return other is CatalogPageArgs &&
        campaignId == other.campaignId &&
        search == other.search &&
        categoryId == other.categoryId;
  }

  @override
  int get hashCode => Object.hash(campaignId, search, categoryId);
}

class CatalogItemPageArgs {
  const CatalogItemPageArgs({
    required this.campaignId,
    required this.itemId,
  });

  final String campaignId;
  final String itemId;

  @override
  bool operator ==(Object other) {
    return other is CatalogItemPageArgs && campaignId == other.campaignId && itemId == other.itemId;
  }

  @override
  int get hashCode => Object.hash(campaignId, itemId);
}

final catalogPageProvider = FutureProvider.family<CatalogPageDto, CatalogPageArgs>((ref, args) async {
  return ref.read(bffApiProvider).getCatalogPage(
        args.campaignId,
        search: args.search,
        categoryId: args.categoryId,
      );
});

final catalogItemPageProvider = FutureProvider.family<CatalogItemPageDto, CatalogItemPageArgs>((ref, args) async {
  return ref.read(bffApiProvider).getCatalogItemPage(args.campaignId, args.itemId);
});
