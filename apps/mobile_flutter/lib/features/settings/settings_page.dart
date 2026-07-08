import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/campaign_models.dart';
import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:dnd_app/core/api/models/invite_models.dart';
import 'package:dnd_app/core/auth/role_permissions.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/settings/settings_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';
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
  static const _invitePageSize = 4;
  static const _inviteRoles = ['Member', 'Treasurer', 'ReadOnly', 'Admin'];
  static const _memberRoles = ['Admin', 'Treasurer', 'Member', 'ReadOnly'];

  final Map<String, String> _createdInviteCodes = {};
  int _inviteTake = _invitePageSize;

  CampaignInvitesPageArgs _invitePageArgs(String campaignId) {
    return CampaignInvitesPageArgs(
      campaignId: campaignId,
      skip: 0,
      take: _inviteTake,
    );
  }

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

      final invite = await ref.read(bffApiProvider).createInvite(
            campaignId: page.campaignId,
            role: selectedRole,
            maxUses: maxUses,
            expiresInDays: expiresInDays,
          );
      if (invite.inviteId.isNotEmpty && invite.code.isNotEmpty) {
        _createdInviteCodes[invite.inviteId] = invite.code;
      }
      ref.invalidate(campaignInvitesProvider(_invitePageArgs(page.campaignId)));

      if (!mounted) {
        return;
      }

      await showDialog<void>(
        context: context,
        builder: (context) {
          return AlertDialog(
            title: const Text('Invite Created'),
            content: SelectableText(
              invite.code.isEmpty
                  ? 'Invite created. The API did not return a code.'
                  : 'Invite Code: ${invite.code}',
            ),
            actions: [
              if (invite.code.isNotEmpty)
                TextButton(
                  onPressed: () => _copyInviteCode(invite.code),
                  child: const Text('Copy Code'),
                ),
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

  Future<void> _revokeInvite(InviteSummaryDto invite) async {
    final confirmed = await showConfirmDialog(
      context: context,
      title: 'Revoke Invite',
      message: 'Revoke this ${invite.role} invite?',
      confirmLabel: 'Revoke',
    );

    if (!confirmed) {
      return;
    }

    try {
      await ref.read(bffApiProvider).revokeInvite(
            campaignId: widget.campaignId,
            inviteId: invite.inviteId,
          );

      _createdInviteCodes.remove(invite.inviteId);
      ref.invalidate(campaignInvitesProvider(_invitePageArgs(widget.campaignId)));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          const SnackBar(content: Text('Invite revoked.')),
        );
      }
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to revoke invite.');
    }
  }

  Future<void> _copyInviteCode(String code) async {
    await Clipboard.setData(ClipboardData(text: code));

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text('Invite code copied.')),
      );
    }
  }

  Future<void> _updateMemberRole(CampaignSettingsMemberDto member, String role) async {
    try {
      await ref.read(bffApiProvider).updateMemberRole(
            campaignId: widget.campaignId,
            userId: member.userId,
            role: role,
          );

      ref.invalidate(settingsPageProvider(widget.campaignId));
      ref.invalidate(campaignHomePageProvider(widget.campaignId));
      if (mounted) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text('${member.displayName} is now $role.')),
        );
      }
    } catch (error) {
      _showError(error, fallbackMessage: 'Unable to update member role.');
    }
  }

  void _showError(Object error, {required String fallbackMessage}) {
    if (!mounted) {
      return;
    }

    final message = error is AppException ? error.message : fallbackMessage;
    ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
  }

  String _inviteStatus(InviteSummaryDto invite) {
    if (invite.revokedAt != null) {
      return 'Revoked';
    }

    final expiresAt = invite.expiresAt;
    if (expiresAt != null && expiresAt.isBefore(DateTime.now())) {
      return 'Expired';
    }

    if (invite.uses >= invite.maxUses) {
      return 'Used up';
    }

    return 'Active';
  }

  bool _canRevokeInvite(InviteSummaryDto invite) => _inviteStatus(invite) == 'Active';

  String _formatDate(DateTime? date) {
    if (date == null) {
      return 'Never';
    }

    return DateFormat.yMMMd().add_Hm().format(date);
  }

  Widget _buildInvitesSection(CampaignSettingsPageDto page) {
    final invitePageArgs = _invitePageArgs(page.campaignId);
    final invites = ref.watch(campaignInvitesProvider(invitePageArgs));

    return InfoCard(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text('Invites', style: Theme.of(context).textTheme.titleMedium),
          const SizedBox(height: 8),
          SecondaryButton(
            label: 'Create Invite',
            onPressed: () => _showCreateInviteDialog(page),
          ),
          const SizedBox(height: 12),
          invites.when(
            loading: () => const Center(child: CircularProgressIndicator()),
            error: (error, _) {
              final message = error is AppException ? error.message : 'Unable to load invites.';
              return Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(message),
                  const SizedBox(height: 8),
                  SecondaryButton(
                    label: 'Retry',
                    onPressed: () => ref.invalidate(campaignInvitesProvider(invitePageArgs)),
                  ),
                ],
              );
            },
            data: (invitePage) {
              final items = invitePage.items;
              if (items.isEmpty) {
                return const Text('No invites created yet.');
              }

              return Column(
                crossAxisAlignment: CrossAxisAlignment.stretch,
                children: [
                  ...items.map(
                    (invite) => _InviteSummaryTile(
                      invite: invite,
                      status: _inviteStatus(invite),
                      expiresAtText: _formatDate(invite.expiresAt),
                      createdAtText: _formatDate(invite.createdAt),
                      onCopyCode: _canRevokeInvite(invite) && _createdInviteCodes.containsKey(invite.inviteId)
                          ? () => _copyInviteCode(_createdInviteCodes[invite.inviteId]!)
                          : null,
                      onRevoke: _canRevokeInvite(invite) ? () => _revokeInvite(invite) : null,
                    ),
                  ),
                  if (items.length < invitePage.totalCount) ...[
                    const SizedBox(height: 8),
                    SecondaryButton(
                      label: 'Show More',
                      onPressed: () {
                        setState(() {
                          _inviteTake += _invitePageSize;
                        });
                      },
                    ),
                  ],
                ],
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _buildMemberTile({
    required CampaignSettingsMemberDto member,
    required bool canManageMembers,
    required String? currentUserId,
  }) {
    final isCurrentUser = currentUserId != null && member.userId == currentUserId;
    final role = member.role.trim();
    final canEditRole = canManageMembers &&
        !isCurrentUser &&
        !member.isPlatformAdmin &&
        role.toLowerCase() != 'owner';

    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      child: ListTile(
        leading: CircleAvatar(
          child: Text(member.displayName.isEmpty ? '?' : member.displayName[0].toUpperCase()),
        ),
        title: Text(member.displayName),
        subtitle: Text('${member.username} - ${member.role}'),
        trailing: member.isPlatformAdmin
            ? const Icon(Icons.verified, color: Color(0xFF0A6CD8))
            : canEditRole
                ? PopupMenuButton<String>(
                    tooltip: 'Change role',
                    icon: const Icon(Icons.manage_accounts_outlined),
                    onSelected: (value) => _updateMemberRole(member, value),
                    itemBuilder: (context) => _memberRoles
                        .where((value) => value != member.role)
                        .map(
                          (value) => PopupMenuItem(
                            value: value,
                            child: Text(value),
                          ),
                        )
                        .toList(),
                  )
                : null,
      ),
    );
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
          final currentUserId = session.user?.userId;

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
                _buildInvitesSection(data),
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
                (member) => _buildMemberTile(
                  member: member,
                  canManageMembers: canManageCampaignInvites,
                  currentUserId: currentUserId,
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

class _InviteSummaryTile extends StatelessWidget {
  const _InviteSummaryTile({
    required this.invite,
    required this.status,
    required this.expiresAtText,
    required this.createdAtText,
    required this.onCopyCode,
    required this.onRevoke,
  });

  final InviteSummaryDto invite;
  final String status;
  final String expiresAtText;
  final String createdAtText;
  final VoidCallback? onCopyCode;
  final VoidCallback? onRevoke;

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        ListTile(
          contentPadding: EdgeInsets.zero,
          leading: Icon(
            _statusIcon(status),
            color: status == 'Active' ? const Color(0xFF00C2A8) : null,
          ),
          title: Text('${invite.role} invite'),
          subtitle: Text(
            '$status - ${invite.uses}/${invite.maxUses} uses\n'
            'Expires: $expiresAtText\n'
            'Created: $createdAtText',
          ),
          isThreeLine: true,
          trailing: onCopyCode == null && onRevoke == null
              ? null
              : Row(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    if (onCopyCode != null)
                      IconButton(
                        tooltip: 'Copy invite code',
                        onPressed: onCopyCode,
                        icon: const Icon(Icons.copy_outlined),
                      ),
                    if (onRevoke != null)
                      IconButton(
                        tooltip: 'Revoke invite',
                        onPressed: onRevoke,
                        icon: const Icon(Icons.block_outlined),
                      ),
                  ],
                ),
        ),
        const Divider(),
      ],
    );
  }

  IconData _statusIcon(String status) {
    switch (status) {
      case 'Active':
        return Icons.check_circle_outline;
      case 'Revoked':
        return Icons.block_outlined;
      case 'Expired':
        return Icons.schedule_outlined;
      default:
        return Icons.remove_circle_outline;
    }
  }
}
