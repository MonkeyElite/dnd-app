// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'auth_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

LoginRequestDto _$LoginRequestDtoFromJson(Map<String, dynamic> json) =>
    LoginRequestDto(
      username: json['username'] as String,
      password: json['password'] as String,
    );

Map<String, dynamic> _$LoginRequestDtoToJson(LoginRequestDto instance) =>
    <String, dynamic>{
      'username': instance.username,
      'password': instance.password,
    };

RegisterInviteRequestDto _$RegisterInviteRequestDtoFromJson(
  Map<String, dynamic> json,
) => RegisterInviteRequestDto(
  inviteCode: json['inviteCode'] as String,
  username: json['username'] as String,
  displayName: json['displayName'] as String,
  password: json['password'] as String,
);

Map<String, dynamic> _$RegisterInviteRequestDtoToJson(
  RegisterInviteRequestDto instance,
) => <String, dynamic>{
  'inviteCode': instance.inviteCode,
  'username': instance.username,
  'displayName': instance.displayName,
  'password': instance.password,
};

AuthUserDto _$AuthUserDtoFromJson(Map<String, dynamic> json) => AuthUserDto(
  userId: json['userId'] as String,
  username: json['username'] as String,
  displayName: json['displayName'] as String,
  isPlatformAdmin: json['isPlatformAdmin'] as bool,
);

Map<String, dynamic> _$AuthUserDtoToJson(AuthUserDto instance) =>
    <String, dynamic>{
      'userId': instance.userId,
      'username': instance.username,
      'displayName': instance.displayName,
      'isPlatformAdmin': instance.isPlatformAdmin,
    };

AuthResponseDto _$AuthResponseDtoFromJson(Map<String, dynamic> json) =>
    AuthResponseDto(
      accessToken: json['accessToken'] as String,
      refreshToken: json['refreshToken'] as String?,
      user: AuthUserDto.fromJson(json['user'] as Map<String, dynamic>),
    );

Map<String, dynamic> _$AuthResponseDtoToJson(AuthResponseDto instance) =>
    <String, dynamic>{
      'accessToken': instance.accessToken,
      'refreshToken': instance.refreshToken,
      'user': instance.user,
    };
