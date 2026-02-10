import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
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
      salesReceiptPageProvider(SalesReceiptArgs(campaignId: campaignId, saleId: saleId)),
    );
    final homePage = ref.watch(campaignHomePageProvider(campaignId));
    final currency = homePage.valueOrNull?.currency;

    return AppScaffold(
      title: 'Sale Receipt',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          salesReceiptPageProvider(SalesReceiptArgs(campaignId: campaignId, saleId: saleId)),
        ),
        onRefresh: () => ref.refresh(
          salesReceiptPageProvider(SalesReceiptArgs(campaignId: campaignId, saleId: saleId)).future,
        ),
        builder: (data) {
          final sale = data.sale;
          final totalText = currency == null
              ? '${sale.totals.totalMinor}'
              : formatMoneyMinorUnits(sale.totals.totalMinor, currency);

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text('Status: ${sale.status}', style: Theme.of(context).textTheme.titleMedium),
                    const SizedBox(height: 6),
                    Text('World day: ${sale.soldWorldDay}'),
                    Text('Customer: ${sale.customerId ?? 'Walk-in'}'),
                    Text('Total: $totalText'),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              Text('Line Items', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              ...sale.lines.map(
                (line) => Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: ListTile(
                    title: Text(line.itemId),
                    subtitle: Text('Qty ${line.quantity.toStringAsFixed(2)} • Unit ${line.unitSoldPriceMinor}'),
                    trailing: Text(
                      currency == null
                          ? '${line.lineSubtotalMinor}'
                          : formatMoneyMinorUnits(line.lineSubtotalMinor, currency),
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
