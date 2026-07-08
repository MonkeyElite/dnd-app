import 'package:cached_network_image/cached_network_image.dart';
import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/catalog_models.dart';
import 'package:dnd_app/core/auth/role_permissions.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/catalog/catalog_image_upload.dart';
import 'package:dnd_app/features/catalog/catalog_item_form_dialog.dart';
import 'package:dnd_app/features/catalog/catalog_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class CatalogItemDetailPage extends ConsumerStatefulWidget {
  const CatalogItemDetailPage({
    super.key,
    required this.campaignId,
    required this.itemId,
  });

  final String campaignId;
  final String itemId;

  @override
  ConsumerState<CatalogItemDetailPage> createState() =>
      _CatalogItemDetailPageState();
}

class _CatalogItemDetailPageState extends ConsumerState<CatalogItemDetailPage> {
  bool _isUpdatingArchive = false;
  bool _isSavingEdit = false;

  Future<void> _showEditItemDialog(CatalogPageItemDto item) async {
    CatalogPageDto catalogPage;
    try {
      catalogPage = await ref.read(
        catalogPageProvider(
          CatalogPageArgs(campaignId: widget.campaignId),
        ).future,
      );
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to load catalog options.');
      return;
    }

    if (!mounted) {
      return;
    }

    if (catalogPage.filters.categories.isEmpty ||
        catalogPage.filters.units.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Create at least one category and one unit before editing an item.',
          ),
        ),
      );
      return;
    }

    final result = await showCatalogItemFormDialog(
      context: context,
      page: catalogPage,
      initialItem: item,
    );

    if (result == null) {
      return;
    }

    if (!mounted) {
      return;
    }

    setState(() => _isSavingEdit = true);

    try {
      final imageAssetId = result.selectedImage == null
          ? result.imageAssetId
          : await CatalogImageUploadService(
              ref.read(bffApiProvider),
            ).uploadCatalogItemImage(
              campaignId: widget.campaignId,
              file: result.selectedImage!,
            );

      await ref
          .read(bffApiProvider)
          .updateCatalogItem(
            campaignId: widget.campaignId,
            itemId: widget.itemId,
            name: result.name,
            description: result.description,
            categoryId: result.categoryId,
            unitId: result.unitId,
            baseValueMinor: result.baseValueMinor,
            defaultListPriceMinor: result.defaultListPriceMinor,
            weight: result.weight,
            imageAssetId: imageAssetId,
            tagIds: result.tagIds,
          );

      ref.invalidate(catalogItemPageProvider);
      ref.invalidate(catalogPageProvider);
      if (mounted) {
        ScaffoldMessenger.of(
          context,
        ).showSnackBar(const SnackBar(content: Text('Item updated.')));
      }
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to update item.');
    } finally {
      if (mounted) {
        setState(() => _isSavingEdit = false);
      }
    }
  }

  Future<void> _setArchived(bool isArchived) async {
    final confirmed = await showConfirmDialog(
      context: context,
      title: isArchived ? 'Archive Item' : 'Restore Item',
      message: isArchived
          ? 'Hide this item from active catalog usage?'
          : 'Restore this item back to active usage?',
      confirmLabel: isArchived ? 'Archive' : 'Restore',
    );

    if (!confirmed) {
      return;
    }

    setState(() => _isUpdatingArchive = true);

    try {
      await ref
          .read(bffApiProvider)
          .setCatalogItemArchived(
            campaignId: widget.campaignId,
            itemId: widget.itemId,
            isArchived: isArchived,
          );

      ref.invalidate(catalogItemPageProvider);
      ref.invalidate(catalogPageProvider);
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(isArchived ? 'Item archived.' : 'Item restored.'),
          ),
        );
      }
    } catch (error) {
      if (!mounted) {
        return;
      }

      final message = error is AppException
          ? error.message
          : 'Unable to update item state.';
      ScaffoldMessenger.of(
        context,
      ).showSnackBar(SnackBar(content: Text(message)));
    } finally {
      if (mounted) {
        setState(() => _isUpdatingArchive = false);
      }
    }
  }

  void _showError(Object error, {required String fallbackMessage}) {
    if (!mounted) {
      return;
    }

    final message = error is AppException ? error.message : fallbackMessage;
    ScaffoldMessenger.of(
      context,
    ).showSnackBar(SnackBar(content: Text(message)));
  }

  @override
  Widget build(BuildContext context) {
    final page = ref.watch(
      catalogItemPageProvider(
        CatalogItemPageArgs(
          campaignId: widget.campaignId,
          itemId: widget.itemId,
        ),
      ),
    );
    final homePage = ref.watch(campaignHomePageProvider(widget.campaignId));
    final currency = homePage.valueOrNull?.currency;
    final session = ref.watch(sessionControllerProvider);
    final canWrite =
        (session.user?.isPlatformAdmin ?? false) ||
        isCampaignWriteRole(homePage.valueOrNull?.myRole);

    return AppScaffold(
      title: 'Item Detail',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(
          catalogItemPageProvider(
            CatalogItemPageArgs(
              campaignId: widget.campaignId,
              itemId: widget.itemId,
            ),
          ),
        ),
        onRefresh: () => ref.refresh(
          catalogItemPageProvider(
            CatalogItemPageArgs(
              campaignId: widget.campaignId,
              itemId: widget.itemId,
            ),
          ).future,
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
                                  child: const Icon(
                                    Icons.inventory_2_outlined,
                                    size: 52,
                                  ),
                                )
                              : CachedNetworkImage(
                                  imageUrl: item.image.url!,
                                  fit: BoxFit.cover,
                                  errorWidget: (_, __, ___) =>
                                      const Icon(Icons.broken_image_outlined),
                                ),
                        ),
                      ),
                    ),
                    const SizedBox(height: 16),
                    Text(
                      item.name,
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                        fontWeight: FontWeight.w700,
                      ),
                    ),
                    const SizedBox(height: 6),
                    Text(item.description ?? 'No description'),
                    const SizedBox(height: 14),
                    _RowLabelValue(label: 'List Price', value: priceText),
                    _RowLabelValue(
                      label: 'Weight',
                      value: item.weight?.toStringAsFixed(2) ?? '-',
                    ),
                    _RowLabelValue(
                      label: 'Category',
                      value: item.category.name,
                    ),
                    _RowLabelValue(label: 'Unit', value: item.unit.name),
                    _RowLabelValue(
                      label: 'Archived',
                      value: item.isArchived ? 'Yes' : 'No',
                    ),
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
              if (canWrite) ...[
                const SizedBox(height: 12),
                SecondaryButton(
                  label: 'Edit Item',
                  onPressed: _isSavingEdit || _isUpdatingArchive
                      ? null
                      : () => _showEditItemDialog(item),
                ),
                const SizedBox(height: 10),
                SecondaryButton(
                  label: item.isArchived ? 'Restore Item' : 'Archive Item',
                  onPressed: _isSavingEdit || _isUpdatingArchive
                      ? null
                      : () => _setArchived(!item.isArchived),
                ),
              ],
            ],
          );
        },
      ),
    );
  }
}

class _RowLabelValue extends StatelessWidget {
  const _RowLabelValue({required this.label, required this.value});

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
          Text(value, style: const TextStyle(fontWeight: FontWeight.w600)),
        ],
      ),
    );
  }
}
