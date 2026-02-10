import 'package:cached_network_image/cached_network_image.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/catalog/catalog_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class CatalogItemDetailPage extends ConsumerWidget {
  const CatalogItemDetailPage({
    super.key,
    required this.campaignId,
    required this.itemId,
  });

  final String campaignId;
  final String itemId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(
      catalogItemPageProvider(CatalogItemPageArgs(campaignId: campaignId, itemId: itemId)),
    );
    final homePage = ref.watch(campaignHomePageProvider(campaignId));
    final currency = homePage.valueOrNull?.currency;

    return AppScaffold(
      title: 'Item Detail',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          catalogItemPageProvider(CatalogItemPageArgs(campaignId: campaignId, itemId: itemId)),
        ),
        onRefresh: () => ref.refresh(
          catalogItemPageProvider(CatalogItemPageArgs(campaignId: campaignId, itemId: itemId)).future,
        ),
        builder: (data) {
          final item = data.item;
          final listPrice = item.defaultListPriceMinor ?? item.baseValueMinor;
          final priceText = currency == null
              ? '$listPrice ${data.currencyCode}'
              : formatMoneyMinorUnits(listPrice, currency);

          return ListView(
            padding: const EdgeInsets.all(20),
            children: [
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Center(
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(18),
                        child: SizedBox(
                          width: 180,
                          height: 180,
                          child: item.image.url == null
                              ? Container(
                                  color: const Color(0xFFF0F4FA),
                                  child: const Icon(Icons.inventory_2_outlined, size: 52),
                                )
                              : CachedNetworkImage(
                                  imageUrl: item.image.url!,
                                  fit: BoxFit.cover,
                                  errorWidget: (_, __, ___) => const Icon(Icons.broken_image_outlined),
                                ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),
                    Text(
                      item.name,
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
                    ),
                    const SizedBox(height: 6),
                    Text(item.description ?? 'No description'),
                    const SizedBox(height: 14),
                    _RowLabelValue(label: 'List Price', value: priceText),
                    _RowLabelValue(label: 'Weight', value: item.weight?.toStringAsFixed(2) ?? '-'),
                    _RowLabelValue(label: 'Category', value: item.category.name),
                    _RowLabelValue(label: 'Unit', value: item.unit.name),
                    _RowLabelValue(label: 'Archived', value: item.isArchived ? 'Yes' : 'No'),
                    const SizedBox(height: 8),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: item.tags
                          .map(
                            (tag) => Chip(
                              label: Text(tag.name),
                              visualDensity: VisualDensity.compact,
                            ),
                          )
                          .toList(),
                    ),
                  ],
                ),
              ),
            ],
          );
        },
      ),
    );
  }
}

class _RowLabelValue extends StatelessWidget {
  const _RowLabelValue({
    required this.label,
    required this.value,
  });

  final String label;
  final String value;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 3),
      child: Row(
        children: [
          Expanded(
            child: Text(
              label,
              style: const TextStyle(color: Color(0xFF516074)),
            ),
          ),
          Text(
            value,
            style: const TextStyle(fontWeight: FontWeight.w600),
          ),
        ],
      ),
    );
  }
}
