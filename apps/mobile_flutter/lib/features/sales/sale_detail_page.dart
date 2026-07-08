import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/currency_utils.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/core/utils/world_date_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/catalog/catalog_providers.dart';
import 'package:dnd_app/features/sales/sales_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class SaleDetailPage extends ConsumerWidget {
  const SaleDetailPage({
    super.key,
    required this.campaignId,
    required this.saleId,
  });

  final String campaignId;
  final String saleId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(
      salesReceiptPageProvider(
        SalesReceiptArgs(campaignId: campaignId, saleId: saleId),
      ),
    );
    final homePage = ref.watch(campaignHomePageProvider(campaignId));
    final catalogPage = ref.watch(
      catalogPageProvider(CatalogPageArgs(campaignId: campaignId)),
    );
    final home = homePage.valueOrNull;
    final currency = home?.currency == null
        ? null
        : currencyWithDndCoinFallbacks(home!.currency);
    final calendar = home?.calendar;
    final catalogItems = catalogPage.valueOrNull?.items ?? [];

    return AppScaffold(
      title: 'Sale Receipt',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          salesReceiptPageProvider(
            SalesReceiptArgs(campaignId: campaignId, saleId: saleId),
          ),
        ),
        onRefresh: () => ref.refresh(
          salesReceiptPageProvider(
            SalesReceiptArgs(campaignId: campaignId, saleId: saleId),
          ).future,
        ),
        builder: (data) {
          final sale = data.sale;
          final itemNames = {
            for (final item in catalogItems) item.itemId: item.name,
          };
          final customers = {
            for (final customer in data.filters.customers)
              customer.customerId: customer.name,
          };
          final storageLocations = {
            for (final location in data.filters.storageLocations)
              location.storageLocationId: location.name,
          };
          final soldDateText = calendar == null
              ? 'Day ${sale.soldWorldDay}'
              : formatWorldDate(
                  worldDay: sale.soldWorldDay,
                  calendar: calendar,
                );
          final totalText = currency == null
              ? '${sale.totals.totalMinor}'
              : formatMoneyMinorUnits(sale.totals.totalMinor, currency);
          final paymentLines = sale.payments.map((payment) {
            final coinText = formatCoinPaymentDetails(payment.details);
            final amountText = currency == null
                ? '${payment.amountMinor}'
                : formatMoneyMinorUnits(payment.amountMinor, currency);
            if (coinText.isNotEmpty) {
              return '$coinText ($amountText)';
            }

            return '${payment.method}: $amountText';
          }).toList();

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Status: ${sale.status}',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 6),
                    Text('Sold: $soldDateText'),
                    Text(
                      'Customer: ${sale.customerId == null ? 'Walk-in' : customers[sale.customerId] ?? sale.customerId}',
                    ),
                    Text(
                      'Storage: ${storageLocations[sale.storageLocationId] ?? sale.storageLocationId}',
                    ),
                    Text('Total: $totalText'),
                    if (paymentLines.isNotEmpty) ...[
                      const SizedBox(height: 6),
                      Text('Paid with: ${paymentLines.join(', ')}'),
                    ],
                  ],
                ),
              ),
              const SizedBox(height: 12),
              Text(
                'Line Items',
                style: Theme.of(context).textTheme.titleMedium,
              ),
              const SizedBox(height: 8),
              ...sale.lines.map(
                (line) => Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: ListTile(
                    title: Text(itemNames[line.itemId] ?? line.itemId),
                    subtitle: Text(
                      'Qty ${line.quantity.toStringAsFixed(2)} - Unit '
                      '${currency == null ? line.unitSoldPriceMinor : formatMoneyMinorUnits(line.unitSoldPriceMinor, currency)}',
                    ),
                    trailing: Text(
                      currency == null
                          ? '${line.lineSubtotalMinor}'
                          : formatMoneyMinorUnits(
                              line.lineSubtotalMinor,
                              currency,
                            ),
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
