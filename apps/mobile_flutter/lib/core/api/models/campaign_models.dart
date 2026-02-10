import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:json_annotation/json_annotation.dart';

part 'campaign_models.g.dart';

@JsonSerializable()
class CampaignSummaryDto {
  CampaignSummaryDto({
    required this.campaignId,
    required this.name,
    required this.role,
  });

  final String campaignId;
  final String name;
  final String role;

  factory CampaignSummaryDto.fromJson(Map<String, dynamic> json) =>
      _$CampaignSummaryDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CampaignSummaryDtoToJson(this);
}

@JsonSerializable()
class CampaignsPageDto {
  CampaignsPageDto({required this.campaigns});

  final List<CampaignSummaryDto> campaigns;

  factory CampaignsPageDto.fromJson(Map<String, dynamic> json) =>
      _$CampaignsPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CampaignsPageDtoToJson(this);
}

@JsonSerializable()
class CampaignHomePageDto {
  CampaignHomePageDto({
    required this.campaignId,
    required this.campaignName,
    required this.campaignDescription,
    required this.myRole,
    required this.currentWorldDay,
    required this.calendar,
    required this.currency,
  });

  final String campaignId;
  final String campaignName;
  final String? campaignDescription;
  final String myRole;
  final int currentWorldDay;
  final CalendarConfigDto calendar;
  final CurrencyConfigDto currency;

  factory CampaignHomePageDto.fromJson(Map<String, dynamic> json) =>
      _$CampaignHomePageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CampaignHomePageDtoToJson(this);
}

@JsonSerializable()
class CampaignSettingsMemberDto {
  CampaignSettingsMemberDto({
    required this.userId,
    required this.username,
    required this.displayName,
    required this.role,
    required this.isPlatformAdmin,
  });

  final String userId;
  final String username;
  final String displayName;
  final String role;
  final bool isPlatformAdmin;

  factory CampaignSettingsMemberDto.fromJson(Map<String, dynamic> json) =>
      _$CampaignSettingsMemberDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CampaignSettingsMemberDtoToJson(this);
}

@JsonSerializable()
class CampaignSettingsPageDto {
  CampaignSettingsPageDto({
    required this.campaignId,
    required this.campaignName,
    required this.campaignDescription,
    required this.myRole,
    required this.calendar,
    required this.currency,
    required this.members,
  });

  final String campaignId;
  final String campaignName;
  final String? campaignDescription;
  final String myRole;
  final CalendarConfigDto calendar;
  final CurrencyConfigDto currency;
  final List<CampaignSettingsMemberDto> members;

  factory CampaignSettingsPageDto.fromJson(Map<String, dynamic> json) =>
      _$CampaignSettingsPageDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CampaignSettingsPageDtoToJson(this);
}
