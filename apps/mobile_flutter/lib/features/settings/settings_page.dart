import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/settings/settings_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:package_info_plus/package_info_plus.dart';

class SettingsPage extends ConsumerWidget {
  const SettingsPage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(settingsPageProvider(campaignId));
    final session = ref.watch(sessionControllerProvider);

    return AppScaffold(
      title: 'Settings',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(settingsPageProvider(campaignId)),
        onRefresh: () => ref.refresh(settingsPageProvider(campaignId).future),
        builder: (data) {
          return ListView(
            padding: const EdgeInsets.all(16),
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
                    const SizedBox(height: 8),
                    Text('Role: ${data.myRole}'),
                    Text('Campaign ID: ${data.campaignId}'),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('Server'),
                    const SizedBox(height: 4),
                    Text(session.baseUrl ?? '-'),
                    const SizedBox(height: 10),
                    SecondaryButton(
                      label: 'Change Server',
                      onPressed: () => context.go('/setup'),
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              InfoCard(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('App Version'),
                    const SizedBox(height: 4),
                    FutureBuilder<PackageInfo>(
                      future: PackageInfo.fromPlatform(),
                      builder: (context, snapshot) {
                        if (!snapshot.hasData) {
                          return const Text('Loading...');
                        }

                        return Text('${snapshot.data!.version} (${snapshot.data!.buildNumber})');
                      },
                    ),
                  ],
                ),
              ),
              const SizedBox(height: 12),
              Text('Members', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              ...data.members.map(
                (member) => Card(
                  margin: const EdgeInsets.only(bottom: 8),
                  child: ListTile(
                    leading: CircleAvatar(
                      child: Text(member.displayName.isEmpty ? '?' : member.displayName[0].toUpperCase()),
                    ),
                    title: Text(member.displayName),
                    subtitle: Text('${member.username} • ${member.role}'),
                    trailing: member.isPlatformAdmin
                        ? const Icon(Icons.verified, color: Color(0xFF0A6CD8))
                        : null,
                  ),
                ),
              ),
              const SizedBox(height: 8),
              PrimaryPillButton(
                label: 'Log Out',
                onPressed: () async {
                  await ref.read(sessionControllerProvider.notifier).logout();
                  if (context.mounted) {
                    context.go('/login');
                  }
                },
              ),
            ],
          );
        },
      ),
    );
  }
}
