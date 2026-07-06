import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/campaign_models.dart';
import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:dnd_app/core/auth/role_permissions.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/settings/settings_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:package_info_plus/package_info_plus.dart';

class SettingsPage extends ConsumerStatefulWidget {
  const SettingsPage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  ConsumerState<SettingsPage> createState() => _SettingsPageState();
}

class _SettingsPageState extends ConsumerState<SettingsPage> {
  static const _inviteRoles = ['Member', 'Treasurer', 'ReadOnly', 'Admin'];

  Future<void> _showCreateInviteDialog(CampaignSettingsPageDto page) async {
    var selectedRole = _inviteRoles.first;
    final maxUsesController = TextEditingController(text: '1');
    final expiresInDaysController = TextEditingController();
    final formKey = GlobalKey<FormState>();

    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return StatefulBuilder(
          builder: (context, setDialogState) {
            return AlertDialog(
              title: const Text('Create Invite'),
              content: Form(
                key: formKey,
                child: Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    DropdownButtonFormField<String>(
                      initialValue: selectedRole,
                      decoration: const InputDecoration(labelText: 'Role'),
                      items: _inviteRoles
                          .map((role) => DropdownMenuItem(value: role, child: Text(role)))
                          .toList(),
                      onChanged: (value) {
                        if (value == null) {
                          return;
                        }

                        setDialogState(() => selectedRole = value);
                      },
                    ),
                    const SizedBox(height: 8),
                    RoundedTextField(
                      controller: maxUsesController,
                      label: 'Max Uses',
                      keyboardType: TextInputType.number,
                      validator: (value) {
                        final parsed = int.tryParse(value?.trim() ?? '');
                        if (parsed == null || parsed <= 0) {
                          return 'Max uses must be greater than 0.';
                        }

                        return null;
                      },
                    ),
                    const SizedBox(height: 8),
                    RoundedTextField(
                      controller: expiresInDaysController,
                      label: 'Expires In Days (optional)',
                      keyboardType: TextInputType.number,
                      validator: (value) {
                        final text = value?.trim() ?? '';
                        if (text.isEmpty) {
                          return null;
                        }

                        final parsed = int.tryParse(text);
                        if (parsed == null || parsed < 1) {
                          return 'When set, must be at least 1 day.';
                        }

                        return null;
                      },
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
                  child: const Text('Create Invite'),
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
      final maxUses = int.parse(maxUsesController.text.trim());
      final expiresText = expiresInDaysController.text.trim();
      final expiresInDays = expiresText.isEmpty ? null : int.parse(expiresText);

      final code = await ref.read(bffApiProvider).createInvite(
            campaignId: page.campaignId,
            role: selectedRole,
            maxUses: maxUses,
            expiresInDays: expiresInDays,
          );

      if (!mounted) {
        return;
      }

      await showDialog<void>(
        context: context,
        builder: (context) {
          return AlertDialog(
            title: const Text('Invite Created'),
            content: SelectableText(
              code.isEmpty ? 'Invite created. The API did not return a code.' : 'Invite Code: $code',
            ),
            actions: [
              FilledButton(
                onPressed: () => Navigator.of(context).pop(),
                child: const Text('Close'),
              ),
            ],
          );
        },
      );
    } catch (error) {
      if (!mounted) {
        return;
      }

      final message = error is AppException ? error.message : 'Unable to create invite.';
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
    }
  }

  Future<void> _showUpdateCalendarDialog(CampaignSettingsPageDto page) async {
    final weekLengthController = TextEditingController(
      text: page.calendar.weekLength.toString(),
    );
    final formKey = GlobalKey<FormState>();

    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Update Calendar'),
          content: Form(
            key: formKey,
            child: RoundedTextField(
              controller: weekLengthController,
              label: 'Week Length',
              keyboardType: TextInputType.number,
              validator: (value) {
                final parsed = int.tryParse(value?.trim() ?? '');
                if (parsed == null || parsed <= 0) {
                  return 'Week length must be greater than 0.';
                }

                return null;
              },
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
              child: const Text('Update'),
            ),
          ],
        );
      },
    );

    if (submit != true) {
      return;
    }

    try {
      final weekLength = int.parse(weekLengthController.text.trim());
      await ref.read(bffApiProvider).updateCampaignCalendar(
            campaignId: page.campaignId,
            calendar: CalendarConfigDto(
              campaignId: page.campaignId,
              weekLength: weekLength,
              months: page.calendar.months,
            ),
          );

      ref.invalidate(settingsPageProvider(widget.campaignId));
      ref.invalidate(campaignHomePageProvider(widget.campaignId));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Calendar updated.')),
        );
      }
    } catch (error) {
      if (!mounted) {
        return;
      }

      final message = error is AppException ? error.message : 'Unable to update calendar.';
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
    }
  }

  Future<void> _showUpdateCurrencyDialog(CampaignSettingsPageDto page) async {
    final codeController = TextEditingController(text: page.currency.currencyCode);
    final minorController = TextEditingController(text: page.currency.minorUnitName);
    final majorController = TextEditingController(text: page.currency.majorUnitName);
    final formKey = GlobalKey<FormState>();

    final submit = await showDialog<bool>(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: const Text('Update Currency'),
          content: Form(
            key: formKey,
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                RoundedTextField(
                  controller: codeController,
                  label: 'Currency Code',
                  validator: (value) {
                    if (value == null || value.trim().isEmpty) {
                      return 'Currency code is required.';
                    }

                    return null;
                  },
                ),
                const SizedBox(height: 8),
                RoundedTextField(
                  controller: majorController,
                  label: 'Major Unit Name',
                  validator: (value) =>
                      (value == null || value.trim().isEmpty) ? 'Major unit is required.' : null,
                ),
                const SizedBox(height: 8),
                RoundedTextField(
                  controller: minorController,
                  label: 'Minor Unit Name',
                  validator: (value) =>
                      (value == null || value.trim().isEmpty) ? 'Minor unit is required.' : null,
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
              child: const Text('Update'),
            ),
          ],
        );
      },
    );

    if (submit != true) {
      return;
    }

    try {
      await ref.read(bffApiProvider).updateCampaignCurrency(
            campaignId: page.campaignId,
            currency: CurrencyConfigDto(
              campaignId: page.campaignId,
              currencyCode: codeController.text.trim().toUpperCase(),
              minorUnitName: minorController.text.trim(),
              majorUnitName: majorController.text.trim(),
              denominations: page.currency.denominations,
            ),
          );

      ref.invalidate(settingsPageProvider(widget.campaignId));
      ref.invalidate(campaignHomePageProvider(widget.campaignId));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Currency updated.')),
        );
      }
    } catch (error) {
      if (!mounted) {
        return;
      }

      final message = error is AppException ? error.message : 'Unable to update currency.';
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
    }
  }

  @override
  Widget build(BuildContext context) {
    final page = ref.watch(settingsPageProvider(widget.campaignId));
    final session = ref.watch(sessionControllerProvider);

    return AppScaffold(
      title: 'Settings',
      child: AsyncPage(
        value: page,
        onRetry: () => ref.invalidate(settingsPageProvider(widget.campaignId)),
        onRefresh: () => ref.refresh(settingsPageProvider(widget.campaignId).future),
        builder: (data) {
          final isPlatformAdmin = session.user?.isPlatformAdmin ?? false;
          final canManageSettings = isPlatformAdmin || canManageCampaignSettings(data.myRole);
          final canManageCampaignInvites = isPlatformAdmin || canManageInvites(data.myRole);

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
              if (canManageSettings)
                InfoCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Campaign Controls', style: Theme.of(context).textTheme.titleMedium),
                      const SizedBox(height: 8),
                      SecondaryButton(
                        label: 'Update Calendar',
                        onPressed: () => _showUpdateCalendarDialog(data),
                      ),
                      const SizedBox(height: 8),
                      SecondaryButton(
                        label: 'Update Currency',
                        onPressed: () => _showUpdateCurrencyDialog(data),
                      ),
                    ],
                  ),
                ),
              if (canManageSettings) const SizedBox(height: 12),
              if (canManageCampaignInvites)
                InfoCard(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text('Invites', style: Theme.of(context).textTheme.titleMedium),
                      const SizedBox(height: 8),
                      SecondaryButton(
                        label: 'Create Invite',
                        onPressed: () => _showCreateInviteDialog(data),
                      ),
                    ],
                  ),
                ),
              if (canManageCampaignInvites) const SizedBox(height: 12),
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
                      onPressed: () => context.push('/setup'),
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
                    context.push('/login');
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
