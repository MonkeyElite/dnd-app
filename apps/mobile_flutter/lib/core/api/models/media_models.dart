import 'package:json_annotation/json_annotation.dart';

part 'media_models.g.dart';

@JsonSerializable()
class RequestUploadActionDto {
  RequestUploadActionDto({
    required this.campaignId,
    required this.purpose,
    required this.fileName,
    required this.contentType,
    required this.sizeBytes,
  });

  final String campaignId;
  final String purpose;
  final String fileName;
  final String contentType;
  final int? sizeBytes;

  factory RequestUploadActionDto.fromJson(Map<String, dynamic> json) =>
      _$RequestUploadActionDtoFromJson(json);
  Map<String, dynamic> toJson() => _$RequestUploadActionDtoToJson(this);
}

@JsonSerializable()
class ConfirmUploadActionDto {
  ConfirmUploadActionDto({
    required this.campaignId,
    required this.assetId,
    required this.sha256,
    required this.sizeBytes,
  });

  final String campaignId;
  final String assetId;
  final String? sha256;
  final int? sizeBytes;

  factory ConfirmUploadActionDto.fromJson(Map<String, dynamic> json) =>
      _$ConfirmUploadActionDtoFromJson(json);
  Map<String, dynamic> toJson() => _$ConfirmUploadActionDtoToJson(this);
}

@JsonSerializable()
class CreateUploadResponseDto {
  CreateUploadResponseDto({
    required this.assetId,
    required this.bucket,
    required this.objectKey,
    required this.uploadUrl,
    required this.expiresAt,
  });

  final String assetId;
  final String bucket;
  final String objectKey;
  final String uploadUrl;
  final DateTime expiresAt;

  factory CreateUploadResponseDto.fromJson(Map<String, dynamic> json) =>
      _$CreateUploadResponseDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CreateUploadResponseDtoToJson(this);
}
