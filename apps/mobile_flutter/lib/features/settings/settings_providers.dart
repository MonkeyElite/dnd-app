import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/campaign_models.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final settingsPageProvider = FutureProvider.family<CampaignSettingsPageDto, String>((ref, campaignId) async {
  return ref.read(bffApiProvider).getSettingsPage(campaignId);
});
