import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/catalog_models.dart';
import 'package:dnd_app/core/auth/role_permissions.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
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

  Future<void> _showCreateActionDialog(CatalogPageDto page) async {
    final selection = await showModalBottomSheet<String>(
      context: context,
      builder: (context) {
        return SafeArea(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              ListTile(
                leading: const Icon(Icons.category_outlined),
                title: const Text('Create Category'),
                onTap: () => Navigator.of(context).pop('category'),
              ),
              ListTile(
                leading: const Icon(Icons.straighten_outlined),
                title: const Text('Create Unit'),
                onTap: () => Navigator.of(context).pop('unit'),
              ),
              ListTile(
                leading: const Icon(Icons.sell_outlined),
                title: const Text('Create Tag'),
                onTap: () => Navigator.of(context).pop('tag'),
              ),
              ListTile(
                leading: const Icon(Icons.inventory_2_outlined),
                title: const Text('Create Item'),
                onTap: () => Navigator.of(context).pop('item'),
              ),
            ],
          ),
        );
      },
    );

    if (selection == 'category') {
      await _promptCreateNamedEntity(
        title: 'Create Category',
        onSubmit: (name) => ref.read(bffApiProvider).createCategory(
              campaignId: widget.campaignId,
              name: name,
            ),
      );
    } else if (selection == 'unit') {
      await _promptCreateNamedEntity(
        title: 'Create Unit',
        onSubmit: (name) => ref.read(bffApiProvider).createUnit(
              campaignId: widget.campaignId,
              name: name,
            ),
      );
    } else if (selection == 'tag') {
      await _promptCreateNamedEntity(
        title: 'Create Tag',
        onSubmit: (name) => ref.read(bffApiProvider).createTag(
              campaignId: widget.campaignId,
              name: name,
            ),
      );
    } else if (selection == 'item') {
      await _showCreateItemDialog(page);
    } else {
      return;
    }

    ref.invalidate(catalogPageProvider);
    ref.invalidate(catalogItemPageProvider);
  }

  Future<void> _promptCreateNamedEntity({
    required String title,
    required Future<String> Function(String name) onSubmit,
  }) async {
    final nameController = TextEditingController();
    final formKey = GlobalKey<FormState>();
    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: Text(title),
          content: Form(
            key: formKey,
            child: RoundedTextField(
              controller: nameController,
              label: 'Name',
              validator: (value) =>
                  (value == null || value.trim().isEmpty) ? 'Name is required.' : null,
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

    if (submit != true) {
      return;
    }

    try {
      await onSubmit(nameController.text.trim());
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('$title completed.')),
        );
      }
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to complete request.');
    }
  }

  Future<void> _showCreateItemDialog(CatalogPageDto page) async {
    if (page.filters.categories.isEmpty || page.filters.units.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Create at least one category and one unit before creating an item.',
          ),
        ),
      );
      return;
    }

    final formKey = GlobalKey<FormState>();
    final nameController = TextEditingController();
    final descriptionController = TextEditingController();
    final baseValueController = TextEditingController(text: '0');
    final defaultListPriceController = TextEditingController();
    final weightController = TextEditingController();
    var categoryId = page.filters.categories.first.categoryId;
    var unitId = page.filters.units.first.unitId;

    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Create Item'),
              content: Form(
                key: formKey,
                child: SingleChildScrollView(
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
                      RoundedTextField(
                        controller: descriptionController,
                        label: 'Description',
                      ),
                      const SizedBox(height: 8),
                      DropdownButtonFormField<String>(
                        initialValue: categoryId,
                        decoration: const InputDecoration(labelText: 'Category'),
                        items: page.filters.categories
                            .map(
                              (category) => DropdownMenuItem(
                                value: category.categoryId,
                                child: Text(category.name),
                              ),
                            )
                            .toList(),
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }

                          setDialogState(() => categoryId = value);
                        },
                      ),
                      const SizedBox(height: 8),
                      DropdownButtonFormField<String>(
                        initialValue: unitId,
                        decoration: const InputDecoration(labelText: 'Unit'),
                        items: page.filters.units
                            .map(
                              (unit) => DropdownMenuItem(
                                value: unit.unitId,
                                child: Text(unit.name),
                              ),
                            )
                            .toList(),
                        onChanged: (value) {
                          if (value == null) {
                            return;
                          }

                          setDialogState(() => unitId = value);
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: baseValueController,
                        label: 'Base Value (minor)',
                        keyboardType: TextInputType.number,
                        validator: (value) {
                          final parsed = int.tryParse(value?.trim() ?? '');
                          if (parsed == null || parsed < 0) {
                            return 'Base value must be >= 0.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: defaultListPriceController,
                        label: 'List Price (minor, optional)',
                        keyboardType: TextInputType.number,
                        validator: (value) {
                          final text = value?.trim() ?? '';
                          if (text.isEmpty) {
                            return null;
                          }

                          final parsed = int.tryParse(text);
                          if (parsed == null || parsed < 0) {
                            return 'List price must be >= 0.';
                          }

                          return null;
                        },
                      ),
                      const SizedBox(height: 8),
                      RoundedTextField(
                        controller: weightController,
                        label: 'Weight (optional)',
                        keyboardType: const TextInputType.numberWithOptions(decimal: true),
                        validator: (value) {
                          final text = value?.trim() ?? '';
                          if (text.isEmpty) {
                            return null;
                          }

                          final parsed = double.tryParse(text);
                          if (parsed == null || parsed < 0) {
                            return 'Weight must be >= 0.';
                          }

                          return null;
                        },
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
      final listPriceText = defaultListPriceController.text.trim();
      final weightText = weightController.text.trim();

      await ref.read(bffApiProvider).createCatalogItem(
            campaignId: widget.campaignId,
            name: nameController.text.trim(),
            description: descriptionController.text.trim().isEmpty
                ? null
                : descriptionController.text.trim(),
            categoryId: categoryId,
            unitId: unitId,
            baseValueMinor: int.parse(baseValueController.text.trim()),
            defaultListPriceMinor: listPriceText.isEmpty ? null : int.parse(listPriceText),
            weight: weightText.isEmpty ? null : double.parse(weightText),
            tagIds: const [],
          );

      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Item created.')),
        );
      }
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to create item.');
    }
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
    final args = CatalogPageArgs(
      campaignId: widget.campaignId,
      search: _search,
      categoryId: _selectedCategoryId,
    );

    final page = ref.watch(catalogPageProvider(args));
    final homePage = ref.watch(campaignHomePageProvider(widget.campaignId));
    final currency = homePage.valueOrNull?.currency;
    final session = ref.watch(sessionControllerProvider);
    final canWrite = (session.user?.isPlatformAdmin ?? false) ||
        isCampaignWriteRole(homePage.valueOrNull?.myRole);

    return AppScaffold(
      title: 'Catalog',
      actions: [
        if (canWrite && page.valueOrNull != null)
          IconButton(
            onPressed: () => _showCreateActionDialog(page.valueOrNull!),
            icon: const Icon(Icons.add_circle_outline),
          ),
        IconButton(
          onPressed: () => context.push('/campaign/${widget.campaignId}/home'),
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
                    onTap: () => context.push(
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
