import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/inventory/inventory_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class InventoryLocationsPage extends ConsumerWidget {
  const InventoryLocationsPage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(inventoryLocationsPageProvider(campaignId));

    return AppScaffold(
      title: 'Locations',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(inventoryLocationsPageProvider(campaignId)),
        onRefresh: () => ref.refresh(inventoryLocationsPageProvider(campaignId).future),
        isEmpty: (data) => data.locations.isEmpty,
        emptyMessage: 'No storage locations found.',
        builder: (data) {
          return ListView.builder(
            padding: const EdgeInsets.all(16),
            itemCount: data.locations.length,
            itemBuilder: (context, index) {
              final location = data.locations[index];
              return Card(
                margin: const EdgeInsets.only(bottom: 10),
                child: ListTile(
                  title: Text(location.name),
                  subtitle: Text(
                    '${location.placeName ?? 'No place'} • ${location.totalQuantity.toStringAsFixed(2)} total',
                  ),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () => context.go(
                    '/campaign/$campaignId/inventory/location/${location.storageLocationId}',
                  ),
                ),
              );
            },
          );
        },
      ),
    );
  }
}
