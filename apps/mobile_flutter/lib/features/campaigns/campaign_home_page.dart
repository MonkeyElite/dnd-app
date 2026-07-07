import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class CampaignHomePage extends ConsumerWidget {
  const CampaignHomePage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(campaignHomePageProvider(campaignId));

    return AppScaffold(
      title: 'Campaign Home',
      backgroundAsset: FantasyAssets.backgroundTurtleAlt,
      backgroundAlignment: Alignment.bottomCenter,
      actions: [
        IconButton(
          onPressed: () => context.push('/campaigns'),
          icon: const Icon(Icons.swap_horiz),
        ),
        IconButton(
          onPressed: () => context.push('/campaign/$campaignId/settings'),
          icon: const Icon(Icons.settings),
        ),
      ],
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(campaignHomePageProvider(campaignId)),
        onRefresh: () => ref.refresh(campaignHomePageProvider(campaignId).future),
        builder: (data) {
          return ListView(
            padding: const EdgeInsets.fromLTRB(20, 18, 20, 24),
            children: [
              CampaignHeroCard(
                title: data.campaignName,
                description: data.campaignDescription,
                date: WorldDateText(
                  worldDay: data.currentWorldDay,
                  calendar: data.calendar,
                  style: const TextStyle(
                    color: FantasyColors.parchment,
                    fontSize: 17,
                    height: 1.35,
                  ),
                ),
                role: data.myRole,
              ),
              const SizedBox(height: 34),
              FantasyNavTile(
                icon: Icons.menu_book_rounded,
                label: 'Catalog',
                onTap: () => context.push('/campaign/$campaignId/catalog'),
              ),
              const SizedBox(height: 18),
              FantasyNavTile(
                icon: Icons.backpack_rounded,
                label: 'Inventory',
                onTap: () => context.push('/campaign/$campaignId/inventory'),
              ),
              const SizedBox(height: 18),
              FantasyNavTile(
                icon: Icons.toll_rounded,
                label: 'Sales',
                onTap: () => context.push('/campaign/$campaignId/sales'),
              ),
            ],
          );
        },
      ),
    );
  }
}
