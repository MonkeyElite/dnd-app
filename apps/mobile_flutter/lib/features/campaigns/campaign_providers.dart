import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/campaign_models.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class NoArgs {
  const NoArgs();
}

const noArgs = NoArgs();

final campaignsPageProvider = FutureProvider.family<CampaignsPageDto, NoArgs>((ref, _) async {
  return ref.read(bffApiProvider).getCampaignsPage();
});

final campaignHomePageProvider = FutureProvider.family<CampaignHomePageDto, String>((ref, campaignId) async {
  return ref.read(bffApiProvider).getCampaignHomePage(campaignId);
});
