import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/inventory/inventory_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class InventorySummaryPage extends ConsumerWidget {
  const InventorySummaryPage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final rowsValue = ref.watch(inventorySummaryViewProvider(campaignId));
    final homePage = ref.watch(campaignHomePageProvider(campaignId));
    final currency = homePage.valueOrNull?.currency;

    return AppScaffold(
      title: 'Inventory',
      actions: [
        IconButton(
          onPressed: () => context.go('/campaign/$campaignId/inventory/locations'),
          icon: const Icon(Icons.location_on_outlined),
        ),
      ],
      child: AsyncPage(
        value: rowsValue,
        onRetry: () => ref.invalidate(inventorySummaryViewProvider(campaignId)),
        onRefresh: () => ref.refresh(inventorySummaryViewProvider(campaignId).future),
        isEmpty: (rows) => rows.isEmpty,
        emptyMessage: 'No inventory data available.',
        builder: (rows) {
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              ...rows.map((row) {
                final valueText = currency == null
                    ? '${row.totalValueMinor}'
                    : formatMoneyMinorUnits(row.totalValueMinor, currency);
                final meta = '${row.categoryName} • ${row.totalQuantity.toStringAsFixed(2)} ${row.unitName}';

                return ItemRowTile(
                  name: row.itemName,
                  meta: meta,
                  priceText: valueText,
                  imageUrl: row.imageUrl,
                );
              }),
            ],
          );
        },
      ),
    );
  }
}
