import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class CampaignSelectPage extends ConsumerWidget {
  const CampaignSelectPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(campaignsPageProvider(noArgs));
    final session = ref.watch(sessionControllerProvider);
    final isPlatformAdmin = session.user?.isPlatformAdmin ?? false;

    return AppScaffold(
      title: 'Campaigns',
      actions: [
        IconButton(
          onPressed: () async {
            await ref.read(sessionControllerProvider.notifier).logout();
            if (context.mounted) {
              context.push('/login');
            }
          },
          icon: const Icon(Icons.logout),
        ),
      ],
      floatingActionButton: isPlatformAdmin
          ? FloatingActionButton.extended(
              onPressed: () => _createCampaign(context, ref),
              icon: const Icon(Icons.add_circle_outline),
              label: const Text('Create'),
            )
          : null,
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

  Future<void> _createCampaign(BuildContext context, WidgetRef ref) async {
    final nameController = TextEditingController();
    final descriptionController = TextEditingController();
    final formKey = GlobalKey<FormState>();

    final shouldSubmit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Create Campaign'),
          content: Form(
            key: formKey,
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
              ],
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

    if (shouldSubmit != true) {
      return;
    }

    try {
      final campaignId = await ref.read(bffApiProvider).createCampaign(
            name: nameController.text.trim(),
            description: descriptionController.text.trim().isEmpty
                ? null
                : descriptionController.text.trim(),
          );

      if (campaignId.isEmpty) {
        throw const AppException(
          type: AppExceptionType.unknown,
          message: 'Campaign created, but response was missing campaignId.',
        );
      }

      await ref.read(sessionControllerProvider.notifier).selectCampaign(campaignId);
      ref.invalidate(campaignsPageProvider(noArgs));

      if (context.mounted) {
        context.go('/campaign/$campaignId/home');
      }
    } catch (error) {
      if (!context.mounted) {
        return;
      }

      final message = error is AppException ? error.message : 'Unable to create campaign.';
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
    }
  }
}
