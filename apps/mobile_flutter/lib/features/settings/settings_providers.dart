import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/campaign_models.dart';
import 'package:dnd_app/core/api/models/invite_models.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final settingsPageProvider = FutureProvider.family<CampaignSettingsPageDto, String>((ref, campaignId) async {
  return ref.read(bffApiProvider).getSettingsPage(campaignId);
});

class CampaignInvitesPageArgs {
  const CampaignInvitesPageArgs({
    required this.campaignId,
    required this.skip,
    required this.take,
  });

  final String campaignId;
  final int skip;
  final int take;

  @override
  bool operator ==(Object other) {
    return other is CampaignInvitesPageArgs &&
        other.campaignId == campaignId &&
        other.skip == skip &&
        other.take == take;
  }

  @override
  int get hashCode => Object.hash(campaignId, skip, take);
}

final campaignInvitesProvider = FutureProvider.family<InviteSummaryPageDto, CampaignInvitesPageArgs>((ref, args) async {
  return ref.read(bffApiProvider).getInvitesPage(
        args.campaignId,
        skip: args.skip,
        take: args.take,
      );
});
