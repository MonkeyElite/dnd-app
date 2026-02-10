import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class CampaignSelectPage extends ConsumerWidget {
  const CampaignSelectPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(campaignsPageProvider(noArgs));

    return AppScaffold(
      title: 'Campaigns',
      actions: [
        IconButton(
          onPressed: () async {
            await ref.read(sessionControllerProvider.notifier).logout();
            if (context.mounted) {
              context.go('/login');
            }
          },
          icon: const Icon(Icons.logout),
        ),
      ],
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(campaignsPageProvider(noArgs)),
        onRefresh: () => ref.refresh(campaignsPageProvider(noArgs).future),
        isEmpty: (data) => data.campaigns.isEmpty,
        emptyMessage: 'No campaigns available for this account.',
        builder: (data) {
          return ListView.builder(
            padding: const EdgeInsets.all(20),
            itemCount: data.campaigns.length,
            itemBuilder: (context, index) {
              final campaign = data.campaigns[index];
              return Card(
                margin: const EdgeInsets.only(bottom: 12),
                child: ListTile(
                  title: Text(campaign.name),
                  subtitle: Text('Role: ${campaign.role}'),
                  trailing: const Icon(Icons.chevron_right),
                  onTap: () async {
                    await ref.read(sessionControllerProvider.notifier).selectCampaign(campaign.campaignId);
                    if (context.mounted) {
                      context.go('/campaign/${campaign.campaignId}/home');
                    }
                  },
                ),
              );
            },
          );
        },
      ),
    );
  }
}
