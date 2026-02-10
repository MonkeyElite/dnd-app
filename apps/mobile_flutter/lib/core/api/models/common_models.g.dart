// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'common_models.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CalendarMonthDto _$CalendarMonthDtoFromJson(Map<String, dynamic> json) =>
    CalendarMonthDto(
      key: json['key'] as String,
      name: json['name'] as String,
      days: (json['days'] as num).toInt(),
    );

Map<String, dynamic> _$CalendarMonthDtoToJson(CalendarMonthDto instance) =>
    <String, dynamic>{
      'key': instance.key,
      'name': instance.name,
      'days': instance.days,
    };

CalendarConfigDto _$CalendarConfigDtoFromJson(Map<String, dynamic> json) =>
    CalendarConfigDto(
      campaignId: json['campaignId'] as String,
      weekLength: (json['weekLength'] as num).toInt(),
      months: (json['months'] as List<dynamic>)
          .map((e) => CalendarMonthDto.fromJson(e as Map<String, dynamic>))
          .toList(),
    );

Map<String, dynamic> _$CalendarConfigDtoToJson(CalendarConfigDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'weekLength': instance.weekLength,
      'months': instance.months,
    };

CurrencyDenominationDto _$CurrencyDenominationDtoFromJson(
  Map<String, dynamic> json,
) => CurrencyDenominationDto(
  name: json['name'] as String,
  multiplier: (json['multiplier'] as num).toInt(),
);

Map<String, dynamic> _$CurrencyDenominationDtoToJson(
  CurrencyDenominationDto instance,
) => <String, dynamic>{
  'name': instance.name,
  'multiplier': instance.multiplier,
};

CurrencyConfigDto _$CurrencyConfigDtoFromJson(Map<String, dynamic> json) =>
    CurrencyConfigDto(
      campaignId: json['campaignId'] as String,
      currencyCode: json['currencyCode'] as String,
      minorUnitName: json['minorUnitName'] as String,
      majorUnitName: json['majorUnitName'] as String,
      denominations: (json['denominations'] as List<dynamic>)
          .map(
            (e) => CurrencyDenominationDto.fromJson(e as Map<String, dynamic>),
          )
          .toList(),
    );

Map<String, dynamic> _$CurrencyConfigDtoToJson(CurrencyConfigDto instance) =>
    <String, dynamic>{
      'campaignId': instance.campaignId,
      'currencyCode': instance.currencyCode,
      'minorUnitName': instance.minorUnitName,
      'majorUnitName': instance.majorUnitName,
      'denominations': instance.denominations,
    };

ErrorResponseDto _$ErrorResponseDtoFromJson(Map<String, dynamic> json) =>
    ErrorResponseDto(message: json['message'] as String);

Map<String, dynamic> _$ErrorResponseDtoToJson(ErrorResponseDto instance) =>
    <String, dynamic>{'message': instance.message};
