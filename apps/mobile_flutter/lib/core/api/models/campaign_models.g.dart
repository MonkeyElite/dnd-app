// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'campaign_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CampaignSummaryDto _$CampaignSummaryDtoFromJson(Map<String, dynamic> json) =>
    CampaignSummaryDto(
      campaignId: json['campaignId'] as String,
      name: json['name'] as String,
      role: json['role'] as String,
    );

Map<String, dynamic> _$CampaignSummaryDtoToJson(CampaignSummaryDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'name': instance.name,
      'role': instance.role,
    };

CampaignsPageDto _$CampaignsPageDtoFromJson(Map<String, dynamic> json) =>
    CampaignsPageDto(
      campaigns: (json['campaigns'] as List<dynamic>)
          .map((e) => CampaignSummaryDto.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$CampaignsPageDtoToJson(CampaignsPageDto instance) =>
    <String, dynamic>{'campaigns': instance.campaigns};

CampaignHomePageDto _$CampaignHomePageDtoFromJson(Map<String, dynamic> json) =>
    CampaignHomePageDto(
      campaignId: json['campaignId'] as String,
      campaignName: json['campaignName'] as String,
      campaignDescription: json['campaignDescription'] as String?,
      myRole: json['myRole'] as String,
      currentWorldDay: (json['currentWorldDay'] as num).toInt(),
      calendar: CalendarConfigDto.fromJson(
        json['calendar'] as Map<String, dynamic>,
      ),
      currency: CurrencyConfigDto.fromJson(
        json['currency'] as Map<String, dynamic>,
      ),
    );

Map<String, dynamic> _$CampaignHomePageDtoToJson(
  CampaignHomePageDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'campaignName': instance.campaignName,
  'campaignDescription': instance.campaignDescription,
  'myRole': instance.myRole,
  'currentWorldDay': instance.currentWorldDay,
  'calendar': instance.calendar,
  'currency': instance.currency,
};

CampaignSettingsMemberDto _$CampaignSettingsMemberDtoFromJson(
  Map<String, dynamic> json,
) => CampaignSettingsMemberDto(
  userId: json['userId'] as String,
  username: json['username'] as String,
  displayName: json['displayName'] as String,
  role: json['role'] as String,
  isPlatformAdmin: json['isPlatformAdmin'] as bool,
);

Map<String, dynamic> _$CampaignSettingsMemberDtoToJson(
  CampaignSettingsMemberDto instance,
) => <String, dynamic>{
  'userId': instance.userId,
  'username': instance.username,
  'displayName': instance.displayName,
  'role': instance.role,
  'isPlatformAdmin': instance.isPlatformAdmin,
};

CampaignSettingsPageDto _$CampaignSettingsPageDtoFromJson(
  Map<String, dynamic> json,
) => CampaignSettingsPageDto(
  campaignId: json['campaignId'] as String,
  campaignName: json['campaignName'] as String,
  campaignDescription: json['campaignDescription'] as String?,
  myRole: json['myRole'] as String,
  calendar: CalendarConfigDto.fromJson(
    json['calendar'] as Map<String, dynamic>,
  ),
  currency: CurrencyConfigDto.fromJson(
    json['currency'] as Map<String, dynamic>,
  ),
  members: (json['members'] as List<dynamic>)
      .map((e) => CampaignSettingsMemberDto.fromJson(e as Map<String, dynamic>))
      .toList(),
);

Map<String, dynamic> _$CampaignSettingsPageDtoToJson(
  CampaignSettingsPageDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'campaignName': instance.campaignName,
  'campaignDescription': instance.campaignDescription,
  'myRole': instance.myRole,
  'calendar': instance.calendar,
  'currency': instance.currency,
  'members': instance.members,
};
