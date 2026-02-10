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
      actions: [
        IconButton(
          onPressed: () => context.go('/campaigns'),
          icon: const Icon(Icons.swap_horiz),
        ),
        IconButton(
          onPressed: () => context.go('/campaign/$campaignId/settings'),
          icon: const Icon(Icons.settings),
        ),
      ],
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(campaignHomePageProvider(campaignId)),
        onRefresh: () => ref.refresh(campaignHomePageProvider(campaignId).future),
        builder: (data) {
          return ListView(
            padding: const EdgeInsets.all(20),
            children: [
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      data.campaignName,
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(fontWeight: FontWeight.w700),
                    ),
                    if ((data.campaignDescription ?? '').isNotEmpty) ...[
                      const SizedBox(height: 4),
                      Text(data.campaignDescription!),
                    ],
                    const SizedBox(height: 10),
                    WorldDateText(
                      worldDay: data.currentWorldDay,
                      calendar: data.calendar,
                      style: const TextStyle(fontWeight: FontWeight.w600),
                    ),
                    const SizedBox(height: 4),
                    Text(
                      'Role: ${data.myRole}',
                      style: const TextStyle(color: Color(0xFF4F6076)),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 16),
              PrimaryPillButton(
                label: 'Catalog',
                onPressed: () => context.go('/campaign/$campaignId/catalog'),
              ),
              const SizedBox(height: 10),
              SecondaryButton(
                label: 'Inventory',
                onPressed: () => context.go('/campaign/$campaignId/inventory'),
              ),
              const SizedBox(height: 10),
              SecondaryButton(
                label: 'Sales',
                onPressed: () => context.go('/campaign/$campaignId/sales'),
              ),
            ],
          );
        },
      ),
    );
  }
}
