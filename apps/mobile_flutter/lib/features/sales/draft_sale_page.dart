import 'package:dnd_app/core/api/models/sales_models.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/sales/sales_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class DraftSalePage extends ConsumerStatefulWidget {
  const DraftSalePage({
    super.key,
    required this.campaignId,
    required this.draftId,
  });

  final String campaignId;
  final String draftId;

  @override
  ConsumerState<DraftSalePage> createState() => _DraftSalePageState();
}

class _DraftSalePageState extends ConsumerState<DraftSalePage> {
  String? _selectedItemId;
  final _quantityController = TextEditingController(text: '1');

  @override
  void dispose() {
    _quantityController.dispose();
    super.dispose();
  }

  Future<void> _addLine(SalesDraftPageDto page) async {
    if (_selectedItemId == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Select an item first.')),
      );
      return;
    }

    final quantity = double.tryParse(_quantityController.text.trim());
    if (quantity == null || quantity <= 0) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Quantity must be greater than 0.')),
      );
      return;
    }

    await ref.read(salesDraftControllerProvider.notifier).addLine(
          SalesDraftAddLineRequestDto(
            campaignId: widget.campaignId,
            draftId: widget.draftId,
            itemId: _selectedItemId!,
            quantity: quantity,
            unitSoldPriceMinor: null,
            unitTrueValueMinor: null,
            discountMinor: 0,
            notes: null,
          ),
        );
  }

  Future<void> _removeLine(SalesLineDto line) async {
    final confirmed = await showConfirmDialog(
      context: context,
      title: 'Remove line',
      message: 'Remove this line from the draft?',
      confirmLabel: 'Remove',
    );

    if (!confirmed) {
      return;
    }

    await ref.read(salesDraftControllerProvider.notifier).removeLine(
          SalesDraftRemoveLineRequestDto(
            campaignId: widget.campaignId,
            draftId: widget.draftId,
            saleLineId: line.saleLineId,
          ),
        );
  }

  Future<void> _editLine(SalesLineDto line) async {
    final qtyController = TextEditingController(text: line.quantity.toStringAsFixed(2));
    final priceController = TextEditingController(text: line.unitSoldPriceMinor.toString());

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Update line'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              RoundedTextField(
                controller: qtyController,
                label: 'Quantity',
                keyboardType: const TextInputType.numberWithOptions(decimal: true),
              ),
              const SizedBox(height: 8),
              RoundedTextField(
                controller: priceController,
                label: 'Unit price (minor)',
                keyboardType: TextInputType.number,
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.of(context).pop(false),
              child: const Text('Cancel'),
            ),
            FilledButton(
              onPressed: () => Navigator.of(context).pop(true),
              child: const Text('Update'),
            ),
          ],
        );
      },
    );

    if (confirmed != true) {
      return;
    }

    final quantity = double.tryParse(qtyController.text.trim());
    final unitPrice = int.tryParse(priceController.text.trim());
    if (quantity == null || quantity <= 0 || unitPrice == null || unitPrice < 0) {
      if (!mounted) {
        return;
      }

      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Invalid quantity or unit price.')),
      );
      return;
    }

    await ref.read(salesDraftControllerProvider.notifier).updateLine(
          SalesDraftUpdateLineRequestDto(
            campaignId: widget.campaignId,
            draftId: widget.draftId,
            saleLineId: line.saleLineId,
            quantity: quantity,
            unitSoldPriceMinor: unitPrice,
            unitTrueValueMinor: line.unitTrueValueMinor ?? unitPrice,
            discountMinor: line.discountMinor,
            notes: line.notes,
          ),
        );
  }

  Future<void> _completeDraft() async {
    final confirmed = await showConfirmDialog(
      context: context,
      title: 'Complete sale',
      message: 'Complete this draft sale now?',
      confirmLabel: 'Complete',
    );

    if (!confirmed) {
      return;
    }

    final saleId = await ref.read(salesDraftControllerProvider.notifier).completeDraft(
          SalesDraftCompleteRequestDto(
            campaignId: widget.campaignId,
            draftId: widget.draftId,
          ),
        );

    if (mounted) {
      context.go('/campaign/${widget.campaignId}/sales/$saleId');
    }
  }

  @override
  Widget build(BuildContext context) {
    final page = ref.watch(
      salesDraftPageProvider(
        SalesDraftArgs(campaignId: widget.campaignId, draftId: widget.draftId),
      ),
    );
    final controllerState = ref.watch(salesDraftControllerProvider);
    final homePage = ref.watch(campaignHomePageProvider(widget.campaignId));
    final currency = homePage.valueOrNull?.currency;

    ref.listen<AsyncValue<void>>(salesDraftControllerProvider, (previous, next) {
      next.whenOrNull(
        error: (error, _) {
          final message = error is AppException ? error.message : 'Unable to update draft.';
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
        },
      );
    });

    return AppScaffold(
      title: 'Draft Sale',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          salesDraftPageProvider(SalesDraftArgs(campaignId: widget.campaignId, draftId: widget.draftId)),
        ),
        onRefresh: () => ref.refresh(
          salesDraftPageProvider(SalesDraftArgs(campaignId: widget.campaignId, draftId: widget.draftId)).future,
        ),
        builder: (data) {
          final itemNames = {for (final item in data.itemOptions) item.itemId: item.name};
          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('World day ${data.draft.soldWorldDay}'),
                    const SizedBox(height: 4),
                    Text('Storage: ${data.draft.storageLocationId}'),
                    const SizedBox(height: 4),
                    Text('Status: ${data.draft.status}'),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Add line', style: Theme.of(context).textTheme.titleMedium),
                    const SizedBox(height: 8),
                    DropdownButtonFormField<String?>(
                      initialValue: _selectedItemId,
                      decoration: const InputDecoration(labelText: 'Item'),
                      items: [
                        const DropdownMenuItem<String?>(value: null, child: Text('Select item')),
                        ...data.itemOptions.map(
                          (item) => DropdownMenuItem<String?>(
                            value: item.itemId,
                            child: Text(item.name),
                          ),
                        ),
                      ],
                      onChanged: (value) => setState(() => _selectedItemId = value),
                    ),
                    const SizedBox(height: 8),
                    RoundedTextField(
                      controller: _quantityController,
                      label: 'Quantity',
                      keyboardType: const TextInputType.numberWithOptions(decimal: true),
                    ),
                    const SizedBox(height: 10),
                    PrimaryPillButton(
                      label: 'Add Item',
                      onPressed: controllerState.isLoading ? null : () => _addLine(data),
                      isLoading: controllerState.isLoading,
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              Text('Lines', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              if (data.draft.lines.isEmpty)
                const Text('No lines yet.')
              else
                ...data.draft.lines.map(
                  (line) {
                    final lineTotalText = currency == null
                        ? '${line.lineSubtotalMinor}'
                        : formatMoneyMinorUnits(line.lineSubtotalMinor, currency);
                    return Card(
                      margin: const EdgeInsets.only(bottom: 8),
                      child: ListTile(
                        title: Text(itemNames[line.itemId] ?? line.itemId),
                        subtitle: Text(
                          'Qty ${line.quantity.toStringAsFixed(2)} • Unit ${line.unitSoldPriceMinor}\n'
                          'Subtotal $lineTotalText',
                        ),
                        isThreeLine: true,
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            IconButton(
                              onPressed: controllerState.isLoading ? null : () => _editLine(line),
                              icon: const Icon(Icons.edit_outlined),
                            ),
                            IconButton(
                              onPressed: controllerState.isLoading ? null : () => _removeLine(line),
                              icon: const Icon(Icons.delete_outline),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
              const SizedBox(height: 10),
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Totals', style: Theme.of(context).textTheme.titleMedium),
                    const SizedBox(height: 6),
                    Text('Subtotal: ${data.draft.totals.subtotalMinor}'),
                    Text('Discount: ${data.draft.totals.discountTotalMinor}'),
                    Text(
                      'Total: ${currency == null ? data.draft.totals.totalMinor : formatMoneyMinorUnits(data.draft.totals.totalMinor, currency)}',
                      style: const TextStyle(fontWeight: FontWeight.w700),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              PrimaryPillButton(
                label: 'Complete Sale',
                onPressed: controllerState.isLoading || data.draft.lines.isEmpty ? null : _completeDraft,
                isLoading: controllerState.isLoading,
              ),
            ],
          );
        },
      ),
    );
  }
}
