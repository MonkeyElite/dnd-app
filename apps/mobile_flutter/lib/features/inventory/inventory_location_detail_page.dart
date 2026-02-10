import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/inventory/inventory_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class InventoryLocationDetailPage extends ConsumerWidget {
  const InventoryLocationDetailPage({
    super.key,
    required this.campaignId,
    required this.locationId,
  });

  final String campaignId;
  final String locationId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(
      inventoryLocationDetailPageProvider(
        InventoryLocationArgs(campaignId: campaignId, locationId: locationId),
      ),
    );
    final homePage = ref.watch(campaignHomePageProvider(campaignId));
    final currency = homePage.valueOrNull?.currency;

    return AppScaffold(
      title: 'Location Detail',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          inventoryLocationDetailPageProvider(
            InventoryLocationArgs(campaignId: campaignId, locationId: locationId),
          ),
        ),
        onRefresh: () => ref.refresh(
          inventoryLocationDetailPageProvider(
            InventoryLocationArgs(campaignId: campaignId, locationId: locationId),
          ).future,
        ),
        builder: (data) {
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      data.storageLocationName,
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
                    ),
                    const SizedBox(height: 4),
                    Text('${data.placeName ?? 'No place'} • ${data.storageLocationType}'),
                    if ((data.storageLocationCode ?? '').isNotEmpty) ...[
                      const SizedBox(height: 2),
                      Text('Code: ${data.storageLocationCode}'),
                    ],
                  ],
                ),
              ),
              const SizedBox(height: 14),
              Text('Lots', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              if (data.lots.isEmpty)
                const Text('No lots for this location.')
              else
                ...data.lots.map(
                  (lot) {
                    final unitCostText = currency == null
                        ? '${lot.unitCostMinor}'
                        : formatMoneyMinorUnits(lot.unitCostMinor, currency);
                    return Card(
                      margin: const EdgeInsets.only(bottom: 8),
                      child: ListTile(
                        title: Text(lot.itemName),
                        subtitle: Text(
                          'Qty ${lot.quantityOnHand.toStringAsFixed(2)} • Day ${lot.acquiredWorldDay}\n$unitCostText',
                        ),
                        isThreeLine: true,
                      ),
                    );
                  },
                ),
              const SizedBox(height: 14),
              Text('Adjustments', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              if (data.adjustments.isEmpty)
                const Text('No adjustment history.')
              else
                ...data.adjustments.map(
                  (adjustment) => Card(
                    margin: const EdgeInsets.only(bottom: 8),
                    child: ListTile(
                      title: Text(adjustment.itemName),
                      subtitle: Text(
                        '${adjustment.reason} • ${adjustment.deltaQuantity > 0 ? '+' : ''}'
                        '${adjustment.deltaQuantity.toStringAsFixed(2)} • Day ${adjustment.worldDay}',
                      ),
                    ),
                  ),
                ),
            ],
          );
        },
      ),
    );
  }
}
