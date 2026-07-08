import 'dart:async';

import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:dnd_app/core/api/models/sales_models.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/currency_utils.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/core/utils/world_date_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/inventory/inventory_providers.dart';
import 'package:dnd_app/features/sales/sales_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class DirectSaleCheckoutPage extends ConsumerStatefulWidget {
  const DirectSaleCheckoutPage({
    super.key,
    required this.campaignId,
    required this.saleId,
  });

  final String campaignId;
  final String saleId;

  @override
  ConsumerState<DirectSaleCheckoutPage> createState() =>
      _DirectSaleCheckoutPageState();
}

class _DirectSaleCheckoutPageState
    extends ConsumerState<DirectSaleCheckoutPage> {
  final _notesController = TextEditingController();
  final _soldWorldDayController = TextEditingController();
  final Map<String, TextEditingController> _coinControllers = {};
  final List<_CheckoutLine> _lines = [];
  Timer? _saveDraftDebounce;

  String? _initializedSaleId;
  String? _selectedStorageLocationId;
  String? _selectedCustomerId;
  bool _isCompleting = false;
  bool _isCanceling = false;
  bool _isSavingDraft = false;

  @override
  void dispose() {
    _saveDraftDebounce?.cancel();
    _notesController.dispose();
    _soldWorldDayController.dispose();
    for (final controller in _coinControllers.values) {
      controller.dispose();
    }

    super.dispose();
  }

  void _initialize(SalesDraftPageDto page) {
    if (_initializedSaleId == page.draft.saleId) {
      return;
    }

    _initializedSaleId = page.draft.saleId;
    _selectedStorageLocationId = page.draft.storageLocationId;
    _selectedCustomerId = page.draft.customerId;
    _notesController.text = page.draft.notes ?? '';
    _soldWorldDayController.text = page.draft.soldWorldDay.toString();
    _lines
      ..clear()
      ..addAll(
        page.draft.lines.map(
          (line) => _CheckoutLine(
            saleLineId: line.saleLineId,
            itemId: line.itemId,
            quantity: line.quantity,
            unitSoldPriceMinor: line.unitSoldPriceMinor,
            unitTrueValueMinor: line.unitTrueValueMinor,
            discountMinor: line.discountMinor,
            notes: line.notes,
          ),
        ),
      );
  }

  void _syncCoinControllers(CurrencyConfigDto currency) {
    final names = currency.denominations
        .map((denomination) => denomination.name)
        .toSet();
    final obsoleteNames = _coinControllers.keys
        .where((name) => !names.contains(name))
        .toList();
    for (final name in obsoleteNames) {
      _coinControllers.remove(name)?.dispose();
    }

    for (final denomination in currency.denominations) {
      _coinControllers.putIfAbsent(
        denomination.name,
        () => TextEditingController(text: '0'),
      );
    }
  }

  void _fillSuggestedCoins(CurrencyConfigDto currency) {
    final suggested = suggestTenderedCoinCounts(_totalMinor, currency);
    for (final denomination in currency.denominations) {
      _coinControllers[denomination.name]?.text =
          (suggested[denomination.name] ?? 0).toString();
    }
  }

  int get _subtotalMinor =>
      _lines.fold(0, (sum, line) => sum + line.lineSubtotalMinor);

  int get _discountTotalMinor =>
      _lines.fold(0, (sum, line) => sum + line.discountMinor);

  int get _totalMinor => _subtotalMinor;

  Map<String, int> _coinCounts(CurrencyConfigDto currency) {
    return {
      for (final denomination in currency.denominations)
        denomination.name:
            int.tryParse(
              _coinControllers[denomination.name]?.text.trim() ?? '',
            ) ??
            0,
    };
  }

  int _coinTotalMinor(CurrencyConfigDto currency) {
    return coinCountsTotalMinor(_coinCounts(currency), currency.denominations);
  }

  Future<void> _showLineDialog(
    SalesDraftPageDto page, {
    int? lineIndex,
    CurrencyConfigDto? currency,
  }) async {
    if (page.itemOptions.isEmpty) {
      _showSnack('No catalog items available.');
      return;
    }

    final existing = lineIndex == null ? null : _lines[lineIndex];
    var itemId = existing?.itemId ?? page.itemOptions.first.itemId;
    var selectedItem = page.itemOptions.firstWhere(
      (option) => option.itemId == itemId,
      orElse: () => page.itemOptions.first,
    );
    final quantityController = TextEditingController(
      text: existing?.quantity.toString() ?? '1',
    );
    final priceControllers = {
      for (final denomination in sortedDenominations(currency!))
        denomination.name: TextEditingController(),
    };
    final discountControllers = {
      for (final denomination in sortedDenominations(currency))
        denomination.name: TextEditingController(),
    };
    final discountPercentController = TextEditingController(text: '0');
    var discountMode = _DiscountMode.raw;

    void setAmountControllers(
      Map<String, TextEditingController> controllers,
      int amountMinor,
    ) {
      final counts = coinCountsFromMinor(amountMinor, currency);
      for (final denomination in sortedDenominations(currency)) {
        controllers[denomination.name]?.text = (counts[denomination.name] ?? 0)
            .toString();
      }
    }

    int readAmountControllers(Map<String, TextEditingController> controllers) {
      return coinCountsTotalMinor({
        for (final denomination in sortedDenominations(currency))
          denomination.name:
              int.tryParse(controllers[denomination.name]?.text.trim() ?? '') ??
              0,
      }, currency.denominations);
    }

    int currentGrossMinor() {
      final quantity = double.tryParse(quantityController.text.trim()) ?? 0;
      return (quantity * readAmountControllers(priceControllers)).round();
    }

    int currentDiscountMinor() {
      if (discountMode == _DiscountMode.raw) {
        return readAmountControllers(discountControllers);
      }

      final percent =
          double.tryParse(discountPercentController.text.trim()) ?? 0;
      return (currentGrossMinor() * (percent / 100)).round();
    }

    setAmountControllers(
      priceControllers,
      existing?.unitSoldPriceMinor ??
          selectedItem.defaultListPriceMinor ??
          selectedItem.baseValueMinor,
    );
    setAmountControllers(discountControllers, existing?.discountMinor ?? 0);
    final notesController = TextEditingController(text: existing?.notes ?? '');
    final formKey = GlobalKey<FormState>();

    final submitted = await showDialog<bool>(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: Text(existing == null ? 'Add Item' : 'Edit Item'),
              content: Form(
                key: formKey,
                child: SingleChildScrollView(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      DropdownButtonFormField<String>(
                        initialValue: itemId,
                        decoration: const InputDecoration(labelText: 'Item'),
                        items: page.itemOptions
                            .map(
                              (option) => DropdownMenuItem(
                                value: option.itemId,
                                child: Text(option.name),
                              ),
                            )
                            .toList(),
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }

                          final selected = page.itemOptions.firstWhere(
                            (option) => option.itemId == value,
                          );
                          setDialogState(() {
                            itemId = value;
                            selectedItem = selected;
                            setAmountControllers(
                              priceControllers,
                              selected.defaultListPriceMinor ??
                                  selected.baseValueMinor,
                            );
                          });
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: quantityController,
                        label: 'Quantity',
                        keyboardType: const TextInputType.numberWithOptions(
                          decimal: true,
                        ),
                        validator: (value) {
                          final parsed = double.tryParse(value?.trim() ?? '');
                          return parsed == null || parsed <= 0
                              ? 'Quantity must be greater than 0.'
                              : null;
                        },
                      ),
                      const SizedBox(height: 8),
                      Align(
                        alignment: Alignment.centerLeft,
                        child: Text(
                          'Unit price',
                          style: Theme.of(context).textTheme.labelLarge,
                        ),
                      ),
                      const SizedBox(height: 8),
                      ...sortedDenominations(currency).map(
                        (denomination) => Padding(
                          padding: const EdgeInsets.only(bottom: 8),
                          child: RoundedTextField(
                            controller: priceControllers[denomination.name]!,
                            label: denomination.name,
                            keyboardType: const TextInputType.numberWithOptions(
                              signed: true,
                            ),
                            onChanged: (_) => setDialogState(() {}),
                            validator: (value) {
                              final parsed = int.tryParse(value?.trim() ?? '');
                              if (parsed == null) {
                                return 'Enter a whole number.';
                              }

                              return parsed < 0
                                  ? 'Unit price coins must be 0 or greater.'
                                  : null;
                            },
                          ),
                        ),
                      ),
                      Text(
                        'Unit total: ${formatMoneyMinorUnits(readAmountControllers(priceControllers), currency)}',
                      ),
                      const SizedBox(height: 12),
                      Align(
                        alignment: Alignment.centerLeft,
                        child: Text(
                          'Discount',
                          style: Theme.of(context).textTheme.labelLarge,
                        ),
                      ),
                      const SizedBox(height: 8),
                      SegmentedButton<_DiscountMode>(
                        segments: const [
                          ButtonSegment(
                            value: _DiscountMode.raw,
                            label: Text('Coins'),
                          ),
                          ButtonSegment(
                            value: _DiscountMode.percent,
                            label: Text('Percent'),
                          ),
                        ],
                        selected: {discountMode},
                        onSelectionChanged: (selection) {
                          setDialogState(() {
                            discountMode = selection.first;
                          });
                        },
                      ),
                      const SizedBox(height: 8),
                      if (discountMode == _DiscountMode.raw)
                        ...sortedDenominations(currency).map(
                          (denomination) => Padding(
                            padding: const EdgeInsets.only(bottom: 8),
                            child: RoundedTextField(
                              controller:
                                  discountControllers[denomination.name]!,
                              label: denomination.name,
                              keyboardType:
                                  const TextInputType.numberWithOptions(
                                    signed: true,
                                  ),
                              onChanged: (_) => setDialogState(() {}),
                              validator: (value) {
                                final parsed = int.tryParse(
                                  value?.trim() ?? '',
                                );
                                return parsed == null
                                    ? 'Enter a whole number.'
                                    : null;
                              },
                            ),
                          ),
                        )
                      else
                        RoundedTextField(
                          controller: discountPercentController,
                          label: 'Discount percent',
                          keyboardType: const TextInputType.numberWithOptions(
                            signed: true,
                            decimal: true,
                          ),
                          onChanged: (_) => setDialogState(() {}),
                          validator: (value) {
                            final parsed = double.tryParse(value?.trim() ?? '');
                            return parsed == null ? 'Enter a number.' : null;
                          },
                        ),
                      const SizedBox(height: 8),
                      Text(
                        'Line total: ${formatMoneyMinorUnits(currentGrossMinor() - currentDiscountMinor(), currency)}',
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
                  child: Text(existing == null ? 'Add' : 'Update'),
                ),
              ],
            );
          },
        );
      },
    );

    if (submitted != true) {
      return;
    }

    final notes = notesController.text.trim();
    final line = _CheckoutLine(
      saleLineId: existing?.saleLineId,
      itemId: itemId,
      quantity: double.parse(quantityController.text.trim()),
      unitSoldPriceMinor: readAmountControllers(priceControllers),
      unitTrueValueMinor: selectedItem.baseValueMinor,
      discountMinor: currentDiscountMinor(),
      notes: notes.isEmpty ? null : notes,
    );

    setState(() {
      if (lineIndex == null) {
        _lines.add(line);
      } else {
        _lines[lineIndex] = line;
      }

      _fillSuggestedCoins(currency);
    });

    await _saveDraft();
  }

  void _scheduleSaveDraft() {
    _saveDraftDebounce?.cancel();
    _saveDraftDebounce = Timer(const Duration(milliseconds: 700), () {
      if (!mounted || _selectedStorageLocationId == null) {
        return;
      }

      final soldWorldDay = int.tryParse(_soldWorldDayController.text.trim());
      if (soldWorldDay == null || soldWorldDay < 0) {
        return;
      }

      _saveDraft(showErrors: false);
    });
  }

  Future<void> _saveDraft({bool showErrors = true}) async {
    final soldWorldDay = int.tryParse(_soldWorldDayController.text.trim());
    if (soldWorldDay == null || soldWorldDay < 0) {
      if (showErrors) {
        _showSnack('Sale date must be a valid world day.');
      }
      return;
    }

    if (_selectedStorageLocationId == null ||
        _selectedStorageLocationId!.isEmpty) {
      if (showErrors) {
        _showSnack('Select a storage location.');
      }
      return;
    }

    setState(() => _isSavingDraft = true);
    try {
      final notes = _notesController.text.trim();
      await ref
          .read(bffApiProvider)
          .updateSale(
            saleId: widget.saleId,
            request: SalesUpdateActionRequestDto(
              campaignId: widget.campaignId,
              soldWorldDay: soldWorldDay,
              storageLocationId: _selectedStorageLocationId!,
              customerId: _selectedCustomerId,
              notes: notes.isEmpty ? null : notes,
              lines: _lines
                  .map(
                    (line) => SalesUpdateLineActionRequestDto(
                      saleLineId: line.saleLineId,
                      itemId: line.itemId,
                      quantity: line.quantity,
                      unitSoldPriceMinor: line.unitSoldPriceMinor,
                      unitTrueValueMinor: line.unitTrueValueMinor,
                      discountMinor: line.discountMinor,
                      notes: line.notes,
                    ),
                  )
                  .toList(),
              payments: const [],
            ),
          );

      ref.invalidate(salesPageProvider(widget.campaignId));
      ref.invalidate(
        salesDraftPageProvider(
          SalesDraftArgs(campaignId: widget.campaignId, draftId: widget.saleId),
        ),
      );
    } catch (error) {
      if (showErrors) {
        final message = error is AppException
            ? error.message
            : 'Unable to save draft.';
        _showSnack(message);
      }
    } finally {
      if (mounted) {
        setState(() => _isSavingDraft = false);
      }
    }
  }

  Future<void> _completeSale(
    SalesDraftPageDto page,
    CurrencyConfigDto currency,
  ) async {
    final soldWorldDay = int.tryParse(_soldWorldDayController.text.trim());
    if (soldWorldDay == null || soldWorldDay < 0) {
      _showSnack('Sale date must be a valid world day.');
      return;
    }

    if (_selectedStorageLocationId == null ||
        _selectedStorageLocationId!.isEmpty) {
      _showSnack('Select a storage location.');
      return;
    }

    if (_lines.isEmpty) {
      _showSnack('Add at least one item before completing the sale.');
      return;
    }

    final coinTotal = _coinTotalMinor(currency);
    if (_coinCounts(currency).values.any((quantity) => quantity < 0)) {
      _showSnack('Coin quantities must be 0 or greater.');
      return;
    }

    if (coinTotal != _totalMinor) {
      _showSnack('Tendered coins must equal the sale total.');
      return;
    }

    setState(() => _isCompleting = true);
    try {
      final notes = _notesController.text.trim();
      await ref
          .read(bffApiProvider)
          .updateSale(
            saleId: widget.saleId,
            request: SalesUpdateActionRequestDto(
              campaignId: widget.campaignId,
              soldWorldDay: soldWorldDay,
              storageLocationId: _selectedStorageLocationId!,
              customerId: _selectedCustomerId,
              notes: notes.isEmpty ? null : notes,
              lines: _lines
                  .map(
                    (line) => SalesUpdateLineActionRequestDto(
                      saleLineId: line.saleLineId,
                      itemId: line.itemId,
                      quantity: line.quantity,
                      unitSoldPriceMinor: line.unitSoldPriceMinor,
                      unitTrueValueMinor: line.unitTrueValueMinor,
                      discountMinor: line.discountMinor,
                      notes: line.notes,
                    ),
                  )
                  .toList(),
              payments: [
                SalesUpdatePaymentActionRequestDto(
                  paymentId: null,
                  method: 'Coin',
                  amountMinor: _totalMinor,
                  details: buildCoinPaymentDetails(
                    currency,
                    _coinCounts(currency),
                  ),
                ),
              ],
            ),
          );
      await ref
          .read(bffApiProvider)
          .completeSale(
            saleId: widget.saleId,
            request: SalesCompleteActionRequestDto(
              campaignId: widget.campaignId,
            ),
          );

      ref.invalidate(salesPageProvider(widget.campaignId));
      ref.invalidate(
        salesDraftPageProvider(
          SalesDraftArgs(campaignId: widget.campaignId, draftId: widget.saleId),
        ),
      );
      ref.invalidate(
        salesReceiptPageProvider(
          SalesReceiptArgs(
            campaignId: widget.campaignId,
            saleId: widget.saleId,
          ),
        ),
      );
      ref.invalidate(inventorySummaryPageProvider(widget.campaignId));
      ref.invalidate(inventorySummaryViewProvider(widget.campaignId));
      ref.invalidate(inventoryLocationsPageProvider(widget.campaignId));

      if (mounted) {
        context.go('/campaign/${widget.campaignId}/sales/${widget.saleId}');
      }
    } catch (error) {
      final message = error is AppException
          ? error.message
          : 'Unable to complete sale.';
      _showSnack(message);
    } finally {
      if (mounted) {
        setState(() => _isCompleting = false);
      }
    }
  }

  Future<void> _cancelSale() async {
    final confirmed = await showConfirmDialog(
      context: context,
      title: 'Cancel Sale',
      message:
          'Cancel this sale and move it to voided sales? Using back will keep it as a draft.',
      confirmLabel: 'Cancel Sale',
    );

    if (!confirmed) {
      return;
    }

    setState(() => _isCanceling = true);
    try {
      await ref
          .read(bffApiProvider)
          .voidSale(
            campaignId: widget.campaignId,
            saleId: widget.saleId,
            reason: 'Canceled during checkout',
          );

      ref.invalidate(salesPageProvider(widget.campaignId));
      ref.invalidate(
        salesDraftPageProvider(
          SalesDraftArgs(campaignId: widget.campaignId, draftId: widget.saleId),
        ),
      );

      if (mounted) {
        context.go('/campaign/${widget.campaignId}/sales');
      }
    } catch (error) {
      final message = error is AppException
          ? error.message
          : 'Unable to cancel sale.';
      _showSnack(message);
    } finally {
      if (mounted) {
        setState(() => _isCanceling = false);
      }
    }
  }

  void _showSnack(String message) {
    if (!mounted) {
      return;
    }

    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final page = ref.watch(
      salesDraftPageProvider(
        SalesDraftArgs(campaignId: widget.campaignId, draftId: widget.saleId),
      ),
    );
    final homePage = ref.watch(campaignHomePageProvider(widget.campaignId));
    final currency = homePage.valueOrNull?.currency;
    final checkoutCurrency = currency == null
        ? null
        : currencyWithDndCoinFallbacks(currency);
    final calendar = homePage.valueOrNull?.calendar;
    final currentWorldDay = homePage.valueOrNull?.currentWorldDay;

    return AppScaffold(
      title: 'New Sale',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          salesDraftPageProvider(
            SalesDraftArgs(
              campaignId: widget.campaignId,
              draftId: widget.saleId,
            ),
          ),
        ),
        onRefresh: () => ref.refresh(
          salesDraftPageProvider(
            SalesDraftArgs(
              campaignId: widget.campaignId,
              draftId: widget.saleId,
            ),
          ).future,
        ),
        builder: (data) {
          _initialize(data);
          if (checkoutCurrency != null) {
            _syncCoinControllers(checkoutCurrency);
            if (_coinTotalMinor(checkoutCurrency) == 0 && _totalMinor > 0) {
              _fillSuggestedCoins(checkoutCurrency);
            }
          }

          final itemNames = {
            for (final item in data.itemOptions) item.itemId: item.name,
          };
          final totalText = checkoutCurrency == null
              ? '$_totalMinor'
              : formatMoneyMinorUnits(_totalMinor, checkoutCurrency);
          final coinTotal = checkoutCurrency == null
              ? 0
              : _coinTotalMinor(checkoutCurrency);
          final coinTotalText = checkoutCurrency == null
              ? '$coinTotal'
              : formatMoneyMinorUnits(coinTotal, checkoutCurrency);
          final canComplete =
              checkoutCurrency != null &&
              !_isCompleting &&
              !_isCanceling &&
              !_isSavingDraft &&
              _lines.isNotEmpty &&
              coinTotal == _totalMinor;

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Sale Details',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    if (_isSavingDraft) ...[
                      const SizedBox(height: 4),
                      const Text(
                        'Saving draft...',
                        style: TextStyle(color: FantasyColors.muted),
                      ),
                    ],
                    const SizedBox(height: 8),
                    RoundedTextField(
                      controller: _soldWorldDayController,
                      label: 'Sale date (world day)',
                      keyboardType: TextInputType.number,
                      onChanged: (_) {
                        setState(() {});
                        _scheduleSaveDraft();
                      },
                    ),
                    if (currentWorldDay != null) ...[
                      const SizedBox(height: 6),
                      Align(
                        alignment: Alignment.centerLeft,
                        child: TextButton.icon(
                          onPressed: () {
                            setState(() {
                              _soldWorldDayController.text = currentWorldDay
                                  .toString();
                            });
                            _saveDraft();
                          },
                          icon: const Icon(Icons.today_outlined),
                          label: const Text('Use current date'),
                        ),
                      ),
                    ],
                    if (calendar != null) ...[
                      const SizedBox(height: 6),
                      Text(
                        formatWorldDate(
                          worldDay:
                              int.tryParse(
                                _soldWorldDayController.text.trim(),
                              ) ??
                              0,
                          calendar: calendar,
                        ),
                      ),
                    ],
                    const SizedBox(height: 8),
                    DropdownButtonFormField<String>(
                      initialValue: _selectedStorageLocationId,
                      decoration: const InputDecoration(
                        labelText: 'Storage location',
                      ),
                      items: data.filters.storageLocations
                          .map(
                            (location) => DropdownMenuItem(
                              value: location.storageLocationId,
                              child: Text(location.name),
                            ),
                          )
                          .toList(),
                      onChanged: (value) {
                        setState(() => _selectedStorageLocationId = value);
                        _saveDraft();
                      },
                    ),
                    const SizedBox(height: 8),
                    DropdownButtonFormField<String?>(
                      initialValue: _selectedCustomerId,
                      decoration: const InputDecoration(labelText: 'Customer'),
                      items: [
                        const DropdownMenuItem<String?>(
                          value: null,
                          child: Text('Walk-in'),
                        ),
                        ...data.filters.customers.map(
                          (customer) => DropdownMenuItem<String?>(
                            value: customer.customerId,
                            child: Text(customer.name),
                          ),
                        ),
                      ],
                      onChanged: (value) {
                        setState(() => _selectedCustomerId = value);
                        _saveDraft();
                      },
                    ),
                    const SizedBox(height: 8),
                    RoundedTextField(
                      controller: _notesController,
                      label: 'Notes (optional)',
                      onChanged: (_) => _scheduleSaveDraft(),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: Text(
                      'Line Items',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                  IconButton(
                    tooltip: 'Add item',
                    onPressed: checkoutCurrency == null
                        ? null
                        : () =>
                              _showLineDialog(data, currency: checkoutCurrency),
                    icon: const Icon(Icons.add_circle_outline),
                  ),
                ],
              ),
              if (_lines.isEmpty)
                const Text('No items added yet.')
              else
                ..._lines.asMap().entries.map((entry) {
                  final index = entry.key;
                  final line = entry.value;
                  final itemName = itemNames[line.itemId] ?? line.itemId;
                  final unitText = checkoutCurrency == null
                      ? '${line.unitSoldPriceMinor}'
                      : formatMoneyMinorUnits(
                          line.unitSoldPriceMinor,
                          checkoutCurrency,
                        );
                  final discountText = checkoutCurrency == null
                      ? '${line.discountMinor}'
                      : formatMoneyMinorUnits(
                          line.discountMinor,
                          checkoutCurrency,
                        );
                  final totalText = checkoutCurrency == null
                      ? '${line.lineSubtotalMinor}'
                      : formatMoneyMinorUnits(
                          line.lineSubtotalMinor,
                          checkoutCurrency,
                        );
                  return Card(
                    margin: const EdgeInsets.only(bottom: 8),
                    child: Padding(
                      padding: const EdgeInsets.fromLTRB(14, 10, 8, 12),
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Row(
                            children: [
                              Expanded(
                                child: Text(
                                  itemName,
                                  style: Theme.of(context).textTheme.titleSmall
                                      ?.copyWith(fontWeight: FontWeight.w700),
                                ),
                              ),
                              const SizedBox(width: 8),
                              Flexible(
                                child: Text(
                                  totalText,
                                  textAlign: TextAlign.right,
                                  softWrap: false,
                                  overflow: TextOverflow.ellipsis,
                                  style: const TextStyle(
                                    fontWeight: FontWeight.w700,
                                  ),
                                ),
                              ),
                              IconButton(
                                tooltip: 'Edit item',
                                onPressed: checkoutCurrency == null
                                    ? null
                                    : () => _showLineDialog(
                                        data,
                                        lineIndex: index,
                                        currency: checkoutCurrency,
                                      ),
                                icon: const Icon(Icons.edit_outlined),
                              ),
                              IconButton(
                                tooltip: 'Remove item',
                                onPressed: () async {
                                  setState(() {
                                    _lines.removeAt(index);
                                    if (checkoutCurrency != null) {
                                      _fillSuggestedCoins(checkoutCurrency);
                                    }
                                  });
                                  await _saveDraft();
                                },
                                icon: const Icon(Icons.delete_outline),
                              ),
                            ],
                          ),
                          const SizedBox(height: 6),
                          Text(
                            'Qty ${line.quantity.toStringAsFixed(2)}',
                            style: const TextStyle(color: FantasyColors.muted),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            'Unit: $unitText',
                            style: const TextStyle(color: FantasyColors.muted),
                          ),
                          const SizedBox(height: 2),
                          Text(
                            'Discount: $discountText',
                            style: const TextStyle(color: FantasyColors.muted),
                          ),
                        ],
                      ),
                    ),
                  );
                }),
              const SizedBox(height: 12),
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Totals',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 6),
                    Text(
                      'Subtotal: ${checkoutCurrency == null ? _subtotalMinor : formatMoneyMinorUnits(_subtotalMinor, checkoutCurrency)}',
                    ),
                    Text(
                      'Discount: ${checkoutCurrency == null ? _discountTotalMinor : formatMoneyMinorUnits(_discountTotalMinor, checkoutCurrency)}',
                    ),
                    Text(
                      'Total: $totalText',
                      style: const TextStyle(fontWeight: FontWeight.w700),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              if (checkoutCurrency != null)
                InfoCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Expanded(
                            child: Text(
                              'Payment',
                              style: Theme.of(context).textTheme.titleMedium,
                            ),
                          ),
                          TextButton(
                            onPressed: () => setState(
                              () => _fillSuggestedCoins(checkoutCurrency),
                            ),
                            child: const Text('Suggest'),
                          ),
                        ],
                      ),
                      const SizedBox(height: 8),
                      ...sortedDenominations(checkoutCurrency).map(
                        (denomination) => Padding(
                          padding: const EdgeInsets.only(bottom: 8),
                          child: RoundedTextField(
                            controller: _coinControllers[denomination.name]!,
                            label: denomination.name,
                            keyboardType: TextInputType.number,
                            onChanged: (_) => setState(() {}),
                          ),
                        ),
                      ),
                      Text('Tendered: $coinTotalText'),
                      if (coinTotal != _totalMinor)
                        Text(
                          'Tendered coins must equal $totalText.',
                          style: TextStyle(
                            color: Theme.of(context).colorScheme.error,
                          ),
                        ),
                    ],
                  ),
                ),
              const SizedBox(height: 12),
              Row(
                children: [
                  Expanded(
                    child: SecondaryButton(
                      label: 'Cancel',
                      onPressed: _isCompleting || _isCanceling || _isSavingDraft
                          ? null
                          : _cancelSale,
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: PrimaryPillButton(
                      label: 'Create',
                      onPressed: canComplete
                          ? () => _completeSale(data, checkoutCurrency)
                          : null,
                      isLoading: _isCompleting,
                    ),
                  ),
                ],
              ),
            ],
          );
        },
      ),
    );
  }
}

enum _DiscountMode { raw, percent }

class _CheckoutLine {
  const _CheckoutLine({
    required this.saleLineId,
    required this.itemId,
    required this.quantity,
    required this.unitSoldPriceMinor,
    required this.unitTrueValueMinor,
    required this.discountMinor,
    required this.notes,
  });

  final String? saleLineId;
  final String itemId;
  final double quantity;
  final int unitSoldPriceMinor;
  final int? unitTrueValueMinor;
  final int discountMinor;
  final String? notes;

  int get lineSubtotalMinor =>
      (quantity * unitSoldPriceMinor).round() - discountMinor;
}
