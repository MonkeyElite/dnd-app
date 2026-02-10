import 'package:json_annotation/json_annotation.dart';

part 'common_models.g.dart';

@JsonSerializable()
class CalendarMonthDto {
  CalendarMonthDto({
    required this.key,
    required this.name,
    required this.days,
  });

  final String key;
  final String name;
  final int days;

  factory CalendarMonthDto.fromJson(Map<String, dynamic> json) =>
      _$CalendarMonthDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CalendarMonthDtoToJson(this);
}

@JsonSerializable()
class CalendarConfigDto {
  CalendarConfigDto({
    required this.campaignId,
    required this.weekLength,
    required this.months,
  });

  final String campaignId;
  final int weekLength;
  final List<CalendarMonthDto> months;

  factory CalendarConfigDto.fromJson(Map<String, dynamic> json) =>
      _$CalendarConfigDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CalendarConfigDtoToJson(this);
}

@JsonSerializable()
class CurrencyDenominationDto {
  CurrencyDenominationDto({
    required this.name,
    required this.multiplier,
  });

  final String name;
  final int multiplier;

  factory CurrencyDenominationDto.fromJson(Map<String, dynamic> json) =>
      _$CurrencyDenominationDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CurrencyDenominationDtoToJson(this);
}

@JsonSerializable()
class CurrencyConfigDto {
  CurrencyConfigDto({
    required this.campaignId,
    required this.currencyCode,
    required this.minorUnitName,
    required this.majorUnitName,
    required this.denominations,
  });

  final String campaignId;
  final String currencyCode;
  final String minorUnitName;
  final String majorUnitName;
  final List<CurrencyDenominationDto> denominations;

  factory CurrencyConfigDto.fromJson(Map<String, dynamic> json) =>
      _$CurrencyConfigDtoFromJson(json);
  Map<String, dynamic> toJson() => _$CurrencyConfigDtoToJson(this);
}

@JsonSerializable()
class ErrorResponseDto {
  ErrorResponseDto({required this.message});

  final String message;

  factory ErrorResponseDto.fromJson(Map<String, dynamic> json) =>
      _$ErrorResponseDtoFromJson(json);
  Map<String, dynamic> toJson() => _$ErrorResponseDtoToJson(this);
}
