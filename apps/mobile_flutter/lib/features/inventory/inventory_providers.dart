import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/inventory_models.dart';
import 'package:dnd_app/features/catalog/catalog_providers.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class InventoryLocationArgs {
  const InventoryLocationArgs({
    required this.campaignId,
    required this.locationId,
  });

  final String campaignId;
  final String locationId;

  @override
  bool operator ==(Object other) {
    return other is InventoryLocationArgs &&
        campaignId == other.campaignId &&
        locationId == other.locationId;
  }

  @override
  int get hashCode => Object.hash(campaignId, locationId);
}

class InventorySummaryViewRow {
  InventorySummaryViewRow({
    required this.itemId,
    required this.itemName,
    required this.imageUrl,
    required this.totalQuantity,
    required this.totalValueMinor,
    required this.unitName,
    required this.categoryName,
  });

  final String itemId;
  final String itemName;
  final String? imageUrl;
  final double totalQuantity;
  final int totalValueMinor;
  final String unitName;
  final String categoryName;
}

final inventorySummaryPageProvider = FutureProvider.family<InventoryPageDto, String>((ref, campaignId) async {
  return ref.read(bffApiProvider).getInventorySummaryPage(campaignId);
});

final inventorySummaryViewProvider =
    FutureProvider.family<List<InventorySummaryViewRow>, String>((ref, campaignId) async {
      final summaryPage = await ref.watch(inventorySummaryPageProvider(campaignId).future);
      final catalogPage = await ref.watch(
        catalogPageProvider(CatalogPageArgs(campaignId: campaignId)).future,
      );

      final itemsById = {for (final item in catalogPage.items) item.itemId: item};
      final grouped = <String, InventorySummaryViewRow>{};

      for (final row in summaryPage.rows) {
        final catalogItem = itemsById[row.itemId];
        if (catalogItem == null) {
          continue;
        }

        final unitPrice = catalogItem.defaultListPriceMinor ?? catalogItem.baseValueMinor;
        final valueForRow = (unitPrice * row.onHandQuantity).round();

        final existing = grouped[row.itemId];
        if (existing == null) {
          grouped[row.itemId] = InventorySummaryViewRow(
            itemId: row.itemId,
            itemName: row.itemName,
            imageUrl: row.image.url,
            totalQuantity: row.onHandQuantity,
            totalValueMinor: valueForRow,
            unitName: row.unitName,
            categoryName: row.categoryName,
          );
          continue;
        }

        grouped[row.itemId] = InventorySummaryViewRow(
          itemId: row.itemId,
          itemName: row.itemName,
          imageUrl: existing.imageUrl ?? row.image.url,
          totalQuantity: existing.totalQuantity + row.onHandQuantity,
          totalValueMinor: existing.totalValueMinor + valueForRow,
          unitName: row.unitName,
          categoryName: row.categoryName,
        );
      }

      final rows = grouped.values.toList()
        ..sort((a, b) => a.itemName.toLowerCase().compareTo(b.itemName.toLowerCase()));

      return rows;
    });

final inventoryLocationsPageProvider =
    FutureProvider.family<InventoryLocationsPageDto, String>((ref, campaignId) async {
      return ref.read(bffApiProvider).getInventoryLocationsPage(campaignId);
    });

final inventoryLocationDetailPageProvider =
    FutureProvider.family<InventoryLocationDetailPageDto, InventoryLocationArgs>((ref, args) async {
      return ref.read(bffApiProvider).getInventoryLocationDetailPage(args.campaignId, args.locationId);
    });
