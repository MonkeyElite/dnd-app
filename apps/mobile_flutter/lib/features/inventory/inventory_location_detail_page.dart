import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/catalog_models.dart';
import 'package:dnd_app/core/auth/role_permissions.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/catalog/catalog_providers.dart';
import 'package:dnd_app/features/inventory/inventory_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class InventoryLocationDetailPage extends ConsumerStatefulWidget {
  const InventoryLocationDetailPage({
    super.key,
    required this.campaignId,
    required this.locationId,
  });

  final String campaignId;
  final String locationId;

  @override
  ConsumerState<InventoryLocationDetailPage> createState() => _InventoryLocationDetailPageState();
}

class _InventoryLocationDetailPageState extends ConsumerState<InventoryLocationDetailPage> {
  static const _adjustmentReasons = [
    'Restock',
    'Sale',
    'Damage',
    'Theft',
    'Spoilage',
    'ManualCorrection',
  ];

  Future<void> _showInventoryWriteActions(List<CatalogPageItemDto> items, int defaultWorldDay) async {
    final action = await showModalBottomSheet<String>(
      context: context,
      builder: (context) {
        return SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ListTile(
                leading: const Icon(Icons.add_box_outlined),
                title: const Text('Add Inventory Lot'),
                onTap: () => Navigator.of(context).pop('lot'),
              ),
              ListTile(
                leading: const Icon(Icons.tune_outlined),
                title: const Text('Create Adjustment'),
                onTap: () => Navigator.of(context).pop('adjustment'),
              ),
            ],
          ),
        );
      },
    );

    if (action == 'lot') {
      await _showAddLotDialog(items, defaultWorldDay);
      return;
    }

    if (action == 'adjustment') {
      await _showAdjustmentDialog(items, defaultWorldDay);
    }
  }

  Future<void> _showAddLotDialog(List<CatalogPageItemDto> items, int defaultWorldDay) async {
    if (items.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('No catalog items available for lot creation.')),
      );
      return;
    }

    final formKey = GlobalKey<FormState>();
    final quantityController = TextEditingController(text: '1');
    final unitCostController = TextEditingController(text: '0');
    final worldDayController = TextEditingController(text: defaultWorldDay.toString());
    final sourceController = TextEditingController();
    final notesController = TextEditingController();
    var itemId = items.first.itemId;

    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Add Inventory Lot'),
              content: Form(
                key: formKey,
                child: SingleChildScrollView(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      DropdownButtonFormField<String>(
                        initialValue: itemId,
                        decoration: const InputDecoration(labelText: 'Item'),
                        items: items
                            .map(
                              (item) => DropdownMenuItem(
                                value: item.itemId,
                                child: Text(item.name),
                              ),
                            )
                            .toList(),
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }

                          setDialogState(() => itemId = value);
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: quantityController,
                        label: 'Quantity',
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        validator: (value) {
                          final parsed = double.tryParse(value?.trim() ?? '');
                          if (parsed == null || parsed <= 0) {
                            return 'Quantity must be greater than 0.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: unitCostController,
                        label: 'Unit Cost (minor)',
                        keyboardType: TextInputType.number,
                        validator: (value) {
                          final parsed = int.tryParse(value?.trim() ?? '');
                          if (parsed == null || parsed < 0) {
                            return 'Unit cost must be >= 0.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: worldDayController,
                        label: 'Acquired World Day',
                        keyboardType: TextInputType.number,
                        validator: (value) {
                          final parsed = int.tryParse(value?.trim() ?? '');
                          if (parsed == null || parsed < 0) {
                            return 'World day must be >= 0.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: sourceController,
                        label: 'Source (optional)',
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: notesController,
                        label: 'Notes (optional)',
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.of(context).pop(false),
                  child: const Text('Cancel'),
                ),
                FilledButton(
                  onPressed: () {
                    if (!formKey.currentState!.validate()) {
                      return;
                    }

                    Navigator.of(context).pop(true);
                  },
                  child: const Text('Create'),
                ),
              ],
            );
          },
        );
      },
    );

    if (submit != true) {
      return;
    }

    try {
      final source = sourceController.text.trim();
      final notes = notesController.text.trim();
      await ref.read(bffApiProvider).createInventoryLot(
            campaignId: widget.campaignId,
            itemId: itemId,
            storageLocationId: widget.locationId,
            quantity: double.parse(quantityController.text.trim()),
            unitCostMinor: int.parse(unitCostController.text.trim()),
            acquiredWorldDay: int.parse(worldDayController.text.trim()),
            source: source.isEmpty ? null : source,
            notes: notes.isEmpty ? null : notes,
          );

      _invalidateInventory();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Inventory lot created.')),
        );
      }
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to create inventory lot.');
    }
  }

  Future<void> _showAdjustmentDialog(List<CatalogPageItemDto> items, int defaultWorldDay) async {
    if (items.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('No catalog items available for adjustments.')),
      );
      return;
    }

    final formKey = GlobalKey<FormState>();
    final deltaController = TextEditingController();
    final worldDayController = TextEditingController(text: defaultWorldDay.toString());
    final notesController = TextEditingController();
    var itemId = items.first.itemId;
    var reason = _adjustmentReasons.last;

    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Create Adjustment'),
              content: Form(
                key: formKey,
                child: SingleChildScrollView(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      DropdownButtonFormField<String>(
                        initialValue: itemId,
                        decoration: const InputDecoration(labelText: 'Item'),
                        items: items
                            .map(
                              (item) => DropdownMenuItem(
                                value: item.itemId,
                                child: Text(item.name),
                              ),
                            )
                            .toList(),
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }

                          setDialogState(() => itemId = value);
                        },
                      ),
                      const SizedBox(height: 8),
                      DropdownButtonFormField<String>(
                        initialValue: reason,
                        decoration: const InputDecoration(labelText: 'Reason'),
                        items: _adjustmentReasons
                            .map((entry) => DropdownMenuItem(value: entry, child: Text(entry)))
                            .toList(),
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }

                          setDialogState(() => reason = value);
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: deltaController,
                        label: 'Delta Quantity',
                        keyboardType: const TextInputType.numberWithOptions(decimal: true, signed: true),
                        validator: (value) {
                          final parsed = double.tryParse(value?.trim() ?? '');
                          if (parsed == null || parsed == 0) {
                            return 'Delta quantity cannot be 0.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: worldDayController,
                        label: 'World Day',
                        keyboardType: TextInputType.number,
                        validator: (value) {
                          final parsed = int.tryParse(value?.trim() ?? '');
                          if (parsed == null || parsed < 0) {
                            return 'World day must be >= 0.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: notesController,
                        label: 'Notes (optional)',
                      ),
                    ],
                  ),
                ),
              ),
              actions: [
                TextButton(
                  onPressed: () => Navigator.of(context).pop(false),
                  child: const Text('Cancel'),
                ),
                FilledButton(
                  onPressed: () {
                    if (!formKey.currentState!.validate()) {
                      return;
                    }

                    Navigator.of(context).pop(true);
                  },
                  child: const Text('Create'),
                ),
              ],
            );
          },
        );
      },
    );

    if (submit != true) {
      return;
    }

    try {
      final notes = notesController.text.trim();
      await ref.read(bffApiProvider).createInventoryAdjustment(
            campaignId: widget.campaignId,
            itemId: itemId,
            storageLocationId: widget.locationId,
            deltaQuantity: double.parse(deltaController.text.trim()),
            reason: reason,
            worldDay: int.parse(worldDayController.text.trim()),
            notes: notes.isEmpty ? null : notes,
          );

      _invalidateInventory();
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Inventory adjustment created.')),
        );
      }
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to create adjustment.');
    }
  }

  void _invalidateInventory() {
    ref.invalidate(
      inventoryLocationDetailPageProvider(
        InventoryLocationArgs(campaignId: widget.campaignId, locationId: widget.locationId),
      ),
    );
    ref.invalidate(inventorySummaryPageProvider(widget.campaignId));
    ref.invalidate(inventorySummaryViewProvider(widget.campaignId));
    ref.invalidate(inventoryLocationsPageProvider(widget.campaignId));
  }

  void _showError(Object error, {required String fallbackMessage}) {
    if (!mounted) {
      return;
    }

    final message = error is AppException ? error.message : fallbackMessage;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final page = ref.watch(
      inventoryLocationDetailPageProvider(
        InventoryLocationArgs(campaignId: widget.campaignId, locationId: widget.locationId),
      ),
    );
    final homePage = ref.watch(campaignHomePageProvider(widget.campaignId));
    final currency = homePage.valueOrNull?.currency;
    final catalogPage = ref.watch(catalogPageProvider(CatalogPageArgs(campaignId: widget.campaignId)));
    final session = ref.watch(sessionControllerProvider);
    final canWrite = (session.user?.isPlatformAdmin ?? false) ||
        isCampaignWriteRole(homePage.valueOrNull?.myRole);
    final worldDay = homePage.valueOrNull?.currentWorldDay ?? 0;

    return AppScaffold(
      title: 'Location Detail',
      floatingActionButton: canWrite
          ? FloatingActionButton(
              onPressed: () => _showInventoryWriteActions(catalogPage.valueOrNull?.items ?? [], worldDay),
              child: const Icon(Icons.add),
            )
          : null,
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          inventoryLocationDetailPageProvider(
            InventoryLocationArgs(campaignId: widget.campaignId, locationId: widget.locationId),
          ),
        ),
        onRefresh: () => ref.refresh(
          inventoryLocationDetailPageProvider(
            InventoryLocationArgs(campaignId: widget.campaignId, locationId: widget.locationId),
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
