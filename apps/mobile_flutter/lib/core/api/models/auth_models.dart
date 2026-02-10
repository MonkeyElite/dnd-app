import 'package:json_annotation/json_annotation.dart';

part 'auth_models.g.dart';

@JsonSerializable()
class LoginRequestDto {
  LoginRequestDto({
    required this.username,
    required this.password,
  });

  final String username;
  final String password;

  factory LoginRequestDto.fromJson(Map<String, dynamic> json) =>
      _$LoginRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$LoginRequestDtoToJson(this);
}

@JsonSerializable()
class RegisterInviteRequestDto {
  RegisterInviteRequestDto({
    required this.inviteCode,
    required this.username,
    required this.displayName,
    required this.password,
  });

  final String inviteCode;
  final String username;
  final String displayName;
  final String password;

  factory RegisterInviteRequestDto.fromJson(Map<String, dynamic> json) =>
      _$RegisterInviteRequestDtoFromJson(json);
  Map<String, dynamic> toJson() => _$RegisterInviteRequestDtoToJson(this);
}

@JsonSerializable()
class AuthUserDto {
  AuthUserDto({
    required this.userId,
    required this.username,
    required this.displayName,
    required this.isPlatformAdmin,
  });

  final String userId;
  final String username;
  final String displayName;
  final bool isPlatformAdmin;

  factory AuthUserDto.fromJson(Map<String, dynamic> json) =>
      _$AuthUserDtoFromJson(json);
  Map<String, dynamic> toJson() => _$AuthUserDtoToJson(this);
}

@JsonSerializable()
class AuthResponseDto {
  AuthResponseDto({
    required this.accessToken,
    required this.refreshToken,
    required this.user,
  });

  final String accessToken;
  final String? refreshToken;
  final AuthUserDto user;

  factory AuthResponseDto.fromJson(Map<String, dynamic> json) =>
      _$AuthResponseDtoFromJson(json);
  Map<String, dynamic> toJson() => _$AuthResponseDtoToJson(this);
}
