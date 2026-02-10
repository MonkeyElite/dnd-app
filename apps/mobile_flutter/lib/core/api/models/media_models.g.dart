// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'media_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

RequestUploadActionDto _$RequestUploadActionDtoFromJson(
  Map<String, dynamic> json,
) => RequestUploadActionDto(
  campaignId: json['campaignId'] as String,
  purpose: json['purpose'] as String,
  fileName: json['fileName'] as String,
  contentType: json['contentType'] as String,
  sizeBytes: (json['sizeBytes'] as num?)?.toInt(),
);

Map<String, dynamic> _$RequestUploadActionDtoToJson(
  RequestUploadActionDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'purpose': instance.purpose,
  'fileName': instance.fileName,
  'contentType': instance.contentType,
  'sizeBytes': instance.sizeBytes,
};

ConfirmUploadActionDto _$ConfirmUploadActionDtoFromJson(
  Map<String, dynamic> json,
) => ConfirmUploadActionDto(
  campaignId: json['campaignId'] as String,
  assetId: json['assetId'] as String,
  sha256: json['sha256'] as String?,
  sizeBytes: (json['sizeBytes'] as num?)?.toInt(),
);

Map<String, dynamic> _$ConfirmUploadActionDtoToJson(
  ConfirmUploadActionDto instance,
) => <String, dynamic>{
  'campaignId': instance.campaignId,
  'assetId': instance.assetId,
  'sha256': instance.sha256,
  'sizeBytes': instance.sizeBytes,
};

CreateUploadResponseDto _$CreateUploadResponseDtoFromJson(
  Map<String, dynamic> json,
) => CreateUploadResponseDto(
  assetId: json['assetId'] as String,
  bucket: json['bucket'] as String,
  objectKey: json['objectKey'] as String,
  uploadUrl: json['uploadUrl'] as String,
  expiresAt: DateTime.parse(json['expiresAt'] as String),
);

Map<String, dynamic> _$CreateUploadResponseDtoToJson(
  CreateUploadResponseDto instance,
) => <String, dynamic>{
  'assetId': instance.assetId,
  'bucket': instance.bucket,
  'objectKey': instance.objectKey,
  'uploadUrl': instance.uploadUrl,
  'expiresAt': instance.expiresAt.toIso8601String(),
};
