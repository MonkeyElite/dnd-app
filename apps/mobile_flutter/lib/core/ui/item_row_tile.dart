import 'package:cached_network_image/cached_network_image.dart';
import 'package:flutter/material.dart';

import 'fantasy_widgets.dart';

class ItemRowTile extends StatelessWidget {
  const ItemRowTile({
    super.key,
    required this.name,
    required this.meta,
    required this.priceText,
    required this.imageUrl,
    this.onTap,
  });

  final String name;
  final String meta;
  final String priceText;
  final String? imageUrl;
  final VoidCallback? onTap;

  @override
  Widget build(BuildContext context) {
    return Container(
      margin: const EdgeInsets.only(bottom: 10),
      decoration: BoxDecoration(
        color: FantasyColors.panel,
        borderRadius: BorderRadius.circular(16),
        border: Border.all(color: FantasyColors.border),
      ),
      child: Material(
        color: Colors.transparent,
        child: ListTile(
          onTap: onTap,
          contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 8),
          leading: ClipRRect(
            borderRadius: BorderRadius.circular(14),
            child: SizedBox(
              width: 52,
              height: 52,
              child: imageUrl == null || imageUrl!.isEmpty
                  ? Container(
                      color: FantasyColors.panelLight,
                      child: const Icon(Icons.inventory_2_outlined, color: FantasyColors.teal),
                    )
                  : CachedNetworkImage(
                      imageUrl: imageUrl!,
                      fit: BoxFit.cover,
                      errorWidget: (_, __, ___) => const Icon(Icons.image_not_supported_outlined),
                    ),
            ),
          ),
          title: Text(
            name,
            maxLines: 1,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(color: FantasyColors.parchment, fontWeight: FontWeight.w800),
          ),
          subtitle: Text(
            meta,
            maxLines: 2,
            overflow: TextOverflow.ellipsis,
            style: const TextStyle(color: FantasyColors.muted),
          ),
          trailing: Text(
            priceText,
            style: const TextStyle(color: FantasyColors.teal, fontWeight: FontWeight.w700),
          ),
        ),
      ),
    );
  }
}
