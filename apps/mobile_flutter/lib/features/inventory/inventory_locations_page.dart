import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/auth/role_permissions.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/inventory/inventory_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class InventoryLocationsPage extends ConsumerStatefulWidget {
  const InventoryLocationsPage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  ConsumerState<InventoryLocationsPage> createState() => _InventoryLocationsPageState();
}

class _InventoryLocationsPageState extends ConsumerState<InventoryLocationsPage> {
  static const _locationTypes = ['Shelf', 'Bin', 'Room', 'Bag', 'Case', 'Other'];

  Future<void> _createLocation() async {
    final nameController = TextEditingController();
    final codeController = TextEditingController();
    final formKey = GlobalKey<FormState>();
    var type = _locationTypes.first;

    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Create Storage Location'),
              content: Form(
                key: formKey,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    RoundedTextField(
                      controller: nameController,
                      label: 'Name',
                      validator: (value) =>
                          (value == null || value.trim().isEmpty) ? 'Name is required.' : null,
                    ),
                    const SizedBox(height: 8),
                    DropdownButtonFormField<String>(
                      initialValue: type,
                      decoration: const InputDecoration(labelText: 'Type'),
                      items: _locationTypes
                          .map((entry) => DropdownMenuItem(value: entry, child: Text(entry)))
                          .toList(),
                      onChanged: (value) {
                        if (value == null) {
                          return;
                        }

                        setDialogState(() => type = value);
                      },
                    ),
                    const SizedBox(height: 8),
                    RoundedTextField(
                      controller: codeController,
                      label: 'Code (optional)',
                    ),
                  ],
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
      final codeText = codeController.text.trim();
      await ref.read(bffApiProvider).createStorageLocation(
            campaignId: widget.campaignId,
            name: nameController.text.trim(),
            type: type,
            code: codeText.isEmpty ? null : codeText,
          );

      ref.invalidate(inventoryLocationsPageProvider(widget.campaignId));
      ref.invalidate(inventorySummaryPageProvider(widget.campaignId));
      ref.invalidate(inventorySummaryViewProvider(widget.campaignId));

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Storage location created.')),
        );
      }
    } catch (error) {
      if (!mounted) {
        return;
      }

      final message = error is AppException ? error.message : 'Unable to create location.';
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
    }
  }

  @override
  Widget build(BuildContext context) {
    final page = ref.watch(inventoryLocationsPageProvider(widget.campaignId));
    final homePage = ref.watch(campaignHomePageProvider(widget.campaignId));
    final session = ref.watch(sessionControllerProvider);
    final canWrite = (session.user?.isPlatformAdmin ?? false) ||
        isCampaignWriteRole(homePage.valueOrNull?.myRole);

    return AppScaffold(
      title: 'Locations',
      actions: [
        IconButton(
          onPressed: () => context.push('/campaign/${widget.campaignId}/home'),
          icon: const Icon(Icons.home_outlined),
        ),
      ],
      floatingActionButton: canWrite
          ? FloatingActionButton.extended(
              onPressed: _createLocation,
              icon: const Icon(Icons.add_location_alt_outlined),
              label: const Text('Add'),
            )
          : null,
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(inventoryLocationsPageProvider(widget.campaignId)),
        onRefresh: () => ref.refresh(inventoryLocationsPageProvider(widget.campaignId).future),
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
                  onTap: () => context.push(
                    '/campaign/${widget.campaignId}/inventory/location/${location.storageLocationId}',
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
