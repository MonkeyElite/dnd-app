import 'dart:typed_data';

import 'package:cached_network_image/cached_network_image.dart';
import 'package:dnd_app/core/api/models/catalog_models.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/catalog/catalog_image_upload.dart';
import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';

class CatalogItemFormResult {
  const CatalogItemFormResult({
    required this.name,
    required this.description,
    required this.categoryId,
    required this.unitId,
    required this.baseValueMinor,
    required this.defaultListPriceMinor,
    required this.weight,
    required this.imageAssetId,
    required this.selectedImage,
    required this.tagIds,
  });

  final String name;
  final String? description;
  final String categoryId;
  final String unitId;
  final int baseValueMinor;
  final int? defaultListPriceMinor;
  final double? weight;
  final String? imageAssetId;
  final XFile? selectedImage;
  final List<String> tagIds;
}

Future<CatalogItemFormResult?> showCatalogItemFormDialog({
  required BuildContext context,
  required CatalogPageDto page,
  CatalogPageItemDto? initialItem,
}) {
  return showDialog<CatalogItemFormResult>(
    context: context,
    builder: (context) => _CatalogItemFormDialog(
      page: page,
      initialItem: initialItem,
    ),
  );
}

class _CatalogItemFormDialog extends StatefulWidget {
  const _CatalogItemFormDialog({
    required this.page,
    required this.initialItem,
  });

  final CatalogPageDto page;
  final CatalogPageItemDto? initialItem;

  @override
  State<_CatalogItemFormDialog> createState() => _CatalogItemFormDialogState();
}

class _CatalogItemFormDialogState extends State<_CatalogItemFormDialog> {
  final _formKey = GlobalKey<FormState>();
  final _nameController = TextEditingController();
  final _descriptionController = TextEditingController();
  final _baseValueController = TextEditingController();
  final _defaultListPriceController = TextEditingController();
  final _weightController = TextEditingController();
  final _imagePicker = ImagePicker();

  late String _categoryId;
  late String _unitId;
  late Set<String> _tagIds;
  String? _imageAssetId;
  XFile? _selectedImage;
  Uint8List? _selectedImageBytes;
  bool _isImageRemoved = false;

  bool get _isEditing => widget.initialItem != null;

  @override
  void initState() {
    super.initState();

    final item = widget.initialItem;
    _nameController.text = item?.name ?? '';
    _descriptionController.text = item?.description ?? '';
    _baseValueController.text = (item?.baseValueMinor ?? 0).toString();
    _defaultListPriceController.text =
        item?.defaultListPriceMinor?.toString() ?? '';
    _weightController.text = item?.weight?.toString() ?? '';
    _categoryId =
        item?.category.categoryId ?? widget.page.filters.categories.first.categoryId;
    _unitId = item?.unit.unitId ?? widget.page.filters.units.first.unitId;
    _tagIds = item?.tags.map((tag) => tag.tagId).toSet() ?? <String>{};
    _imageAssetId = item?.image.assetId;
  }

  @override
  void dispose() {
    _nameController.dispose();
    _descriptionController.dispose();
    _baseValueController.dispose();
    _defaultListPriceController.dispose();
    _weightController.dispose();
    super.dispose();
  }

  Future<void> _chooseImage() async {
    final image = await _imagePicker.pickImage(source: ImageSource.gallery);
    if (image == null) {
      return;
    }

    try {
      CatalogImageUploadService.resolveContentType(image);
      final bytes = await image.readAsBytes();
      setState(() {
        _selectedImage = image;
        _selectedImageBytes = bytes;
        _imageAssetId = null;
        _isImageRemoved = false;
      });
    } on AppException catch (error) {
      if (!mounted) {
        return;
      }

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(error.message)),
      );
    }
  }

  void _removeImage() {
    setState(() {
      _selectedImage = null;
      _selectedImageBytes = null;
      _imageAssetId = null;
      _isImageRemoved = true;
    });
  }

  void _submit() {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    final listPriceText = _defaultListPriceController.text.trim();
    final weightText = _weightController.text.trim();
    final description = _descriptionController.text.trim();

    Navigator.of(context).pop(
      CatalogItemFormResult(
        name: _nameController.text.trim(),
        description: description.isEmpty ? null : description,
        categoryId: _categoryId,
        unitId: _unitId,
        baseValueMinor: int.parse(_baseValueController.text.trim()),
        defaultListPriceMinor: listPriceText.isEmpty
            ? null
            : int.parse(listPriceText),
        weight: weightText.isEmpty ? null : double.parse(weightText),
        imageAssetId: _imageAssetId,
        selectedImage: _selectedImage,
        tagIds: _tagIds.toList(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    final mediaQuery = MediaQuery.of(context);
    final availableHeight =
        mediaQuery.size.height -
        mediaQuery.viewInsets.bottom -
        mediaQuery.padding.vertical -
        220;
    final contentHeight = availableHeight.clamp(300.0, 560.0).toDouble();

    return AlertDialog(
      insetPadding: const EdgeInsets.symmetric(horizontal: 20, vertical: 24),
      clipBehavior: Clip.antiAlias,
      title: Text(_isEditing ? 'Edit Item' : 'Create Item'),
      content: Form(
        key: _formKey,
        child: SizedBox(
          width: double.maxFinite,
          height: contentHeight,
          child: Scrollbar(
            child: SingleChildScrollView(
              padding: const EdgeInsets.only(bottom: 12),
              child: Column(
                mainAxisSize: MainAxisSize.min,
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  _ImagePickerPreview(
                    selectedImageBytes: _selectedImageBytes,
                    currentImageUrl: _selectedImage == null && !_isImageRemoved
                        ? widget.initialItem?.image.url
                        : null,
                    hasImage:
                        _selectedImageBytes != null ||
                        (!_isImageRemoved &&
                            (_imageAssetId != null ||
                                widget.initialItem?.image.url != null)),
                    onChooseImage: _chooseImage,
                    onRemoveImage:
                        _selectedImageBytes != null ||
                            (!_isImageRemoved &&
                                (_imageAssetId != null ||
                                    widget.initialItem?.image.url != null))
                        ? _removeImage
                        : null,
                  ),
                  const SizedBox(height: 12),
                  RoundedTextField(
                    controller: _nameController,
                    label: 'Name',
                    validator: (value) => (value == null || value.trim().isEmpty)
                        ? 'Name is required.'
                        : null,
                  ),
                  const SizedBox(height: 8),
                  RoundedTextField(
                    controller: _descriptionController,
                    label: 'Description',
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<String>(
                    initialValue: _categoryId,
                    decoration: const InputDecoration(labelText: 'Category'),
                    items: widget.page.filters.categories
                        .map(
                          (category) => DropdownMenuItem(
                            value: category.categoryId,
                            child: Text(category.name),
                          ),
                        )
                        .toList(),
                    onChanged: (value) {
                      if (value != null) {
                        setState(() => _categoryId = value);
                      }
                    },
                  ),
                  const SizedBox(height: 8),
                  DropdownButtonFormField<String>(
                    initialValue: _unitId,
                    decoration: const InputDecoration(labelText: 'Unit'),
                    items: widget.page.filters.units
                        .map(
                          (unit) => DropdownMenuItem(
                            value: unit.unitId,
                            child: Text(unit.name),
                          ),
                        )
                        .toList(),
                    onChanged: (value) {
                      if (value != null) {
                        setState(() => _unitId = value);
                      }
                    },
                  ),
                  const SizedBox(height: 8),
                  RoundedTextField(
                    controller: _baseValueController,
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
                    controller: _defaultListPriceController,
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
                    controller: _weightController,
                    label: 'Weight (optional)',
                    keyboardType: const TextInputType.numberWithOptions(
                      decimal: true,
                    ),
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
                  if (widget.page.filters.tags.isNotEmpty) ...[
                    const SizedBox(height: 14),
                    Text(
                      'Tags',
                      style: Theme.of(context).textTheme.labelLarge,
                    ),
                    const SizedBox(height: 8),
                    Wrap(
                      spacing: 8,
                      runSpacing: 8,
                      children: widget.page.filters.tags.map((tag) {
                        final selected = _tagIds.contains(tag.tagId);
                        return FilterChip(
                          label: Text(tag.name),
                          selected: selected,
                          onSelected: (value) {
                            setState(() {
                              if (value) {
                                _tagIds.add(tag.tagId);
                              } else {
                                _tagIds.remove(tag.tagId);
                              }
                            });
                          },
                        );
                      }).toList(),
                    ),
                  ],
                ],
              ),
            ),
          ),
        ),
      ),
      actions: [
        TextButton(
          style: TextButton.styleFrom(
            fixedSize: const Size(88, 40),
            padding: EdgeInsets.zero,
            tapTargetSize: MaterialTapTargetSize.shrinkWrap,
          ),
          onPressed: () => Navigator.of(context).pop(),
          child: const Text('Cancel'),
        ),
        FilledButton(
          style: FilledButton.styleFrom(
            fixedSize: const Size(102, 40),
            padding: EdgeInsets.zero,
            tapTargetSize: MaterialTapTargetSize.shrinkWrap,
          ),
          onPressed: _submit,
          child: Text(_isEditing ? 'Save' : 'Create'),
        ),
      ],
    );
  }
}

class _ImagePickerPreview extends StatelessWidget {
  const _ImagePickerPreview({
    required this.selectedImageBytes,
    required this.currentImageUrl,
    required this.hasImage,
    required this.onChooseImage,
    required this.onRemoveImage,
  });

  final Uint8List? selectedImageBytes;
  final String? currentImageUrl;
  final bool hasImage;
  final VoidCallback onChooseImage;
  final VoidCallback? onRemoveImage;

  @override
  Widget build(BuildContext context) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.stretch,
      children: [
        ClipRRect(
          borderRadius: BorderRadius.circular(12),
          child: SizedBox(
            height: 150,
            child: _buildPreview(),
          ),
        ),
        const SizedBox(height: 8),
        Row(
          children: [
            Expanded(
              child: OutlinedButton.icon(
                onPressed: onChooseImage,
                icon: const Icon(Icons.photo_library_outlined),
                label: const Text('Choose Image'),
              ),
            ),
            if (hasImage) ...[
              const SizedBox(width: 8),
              IconButton(
                tooltip: 'Remove image',
                onPressed: onRemoveImage,
                icon: const Icon(Icons.delete_outline),
              ),
            ],
          ],
        ),
      ],
    );
  }

  Widget _buildPreview() {
    if (selectedImageBytes != null) {
      return Image.memory(selectedImageBytes!, fit: BoxFit.cover);
    }

    if (currentImageUrl != null) {
      return CachedNetworkImage(
        imageUrl: currentImageUrl!,
        fit: BoxFit.cover,
        errorWidget: (_, __, ___) => _placeholder(),
      );
    }

    return _placeholder();
  }

  Widget _placeholder() {
    return Container(
      color: FantasyColors.panelLight,
      alignment: Alignment.center,
      child: const Icon(
        Icons.inventory_2_outlined,
        color: FantasyColors.teal,
        size: 42,
      ),
    );
  }
}
