import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/catalog/catalog_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class CatalogListPage extends ConsumerStatefulWidget {
  const CatalogListPage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  ConsumerState<CatalogListPage> createState() => _CatalogListPageState();
}

class _CatalogListPageState extends ConsumerState<CatalogListPage> {
  final _searchController = TextEditingController();
  String? _search;
  String? _selectedCategoryId;
  String? _selectedTagId;

  @override
  void dispose() {
    _searchController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final args = CatalogPageArgs(
      campaignId: widget.campaignId,
      search: _search,
      categoryId: _selectedCategoryId,
    );

    final page = ref.watch(catalogPageProvider(args));
    final homePage = ref.watch(campaignHomePageProvider(widget.campaignId));
    final currency = homePage.valueOrNull?.currency;

    return AppScaffold(
      title: 'Catalog',
      actions: [
        IconButton(
          onPressed: () => context.go('/campaign/${widget.campaignId}/home'),
          icon: const Icon(Icons.home_outlined),
        ),
      ],
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(catalogPageProvider(args)),
        onRefresh: () => ref.refresh(catalogPageProvider(args).future),
        builder: (data) {
          final filteredItems = data.items
              .where(
                (item) => _selectedTagId == null ||
                    item.tags.any((tag) => tag.tagId == _selectedTagId),
              )
              .toList();

          return ListView(
            padding: const EdgeInsets.all(16),
            children: [
              SearchBarWidget(
                controller: _searchController,
                hintText: 'Search items',
                onSubmitted: (value) => setState(() => _search = value.trim().isEmpty ? null : value.trim()),
                onCleared: () => setState(() => _search = null),
              ),
              const SizedBox(height: 10),
              Row(
                children: [
                  Expanded(
                    child: DropdownButtonFormField<String?>(
                      initialValue: _selectedCategoryId,
                      decoration: const InputDecoration(labelText: 'Category'),
                      items: [
                        const DropdownMenuItem<String?>(value: null, child: Text('All categories')),
                        ...data.filters.categories.map(
                          (category) => DropdownMenuItem<String?>(
                            value: category.categoryId,
                            child: Text(category.name),
                          ),
                        ),
                      ],
                      onChanged: (value) => setState(() => _selectedCategoryId = value),
                    ),
                  ),
                  const SizedBox(width: 10),
                  Expanded(
                    child: DropdownButtonFormField<String?>(
                      initialValue: _selectedTagId,
                      decoration: const InputDecoration(labelText: 'Tag'),
                      items: [
                        const DropdownMenuItem<String?>(value: null, child: Text('All tags')),
                        ...data.filters.tags.map(
                          (tag) => DropdownMenuItem<String?>(
                            value: tag.tagId,
                            child: Text(tag.name),
                          ),
                        ),
                      ],
                      onChanged: (value) => setState(() => _selectedTagId = value),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: 12),
              if (filteredItems.isEmpty)
                const Padding(
                  padding: EdgeInsets.symmetric(vertical: 40),
                  child: Center(child: Text('No items found for the selected filters.')),
                )
              else
                ...filteredItems.map((item) {
                  final price = item.defaultListPriceMinor ?? item.baseValueMinor;
                  final priceText = currency == null
                      ? '$price ${data.currencyCode}'
                      : formatMoneyMinorUnits(price, currency);
                  final tags = item.tags.take(2).map((tag) => tag.name).join(', ');
                  final meta = tags.isEmpty ? item.category.name : '${item.category.name} • $tags';

                  return ItemRowTile(
                    name: item.name,
                    meta: meta,
                    priceText: priceText,
                    imageUrl: item.image.url,
                    onTap: () => context.go(
                      '/campaign/${widget.campaignId}/catalog/item/${item.itemId}',
                    ),
                  );
                }),
            ],
          );
        },
      ),
    );
  }
}
