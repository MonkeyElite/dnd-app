class CreateInviteResultDto {
  CreateInviteResultDto({
    required this.inviteId,
    required this.code,
  });

  final String inviteId;
  final String code;

  factory CreateInviteResultDto.fromJson(Map<String, dynamic> json) {
    return CreateInviteResultDto(
      inviteId: json['inviteId']?.toString() ?? '',
      code: json['code']?.toString() ?? '',
    );
  }
}

class InviteSummaryDto {
  InviteSummaryDto({
    required this.inviteId,
    required this.role,
    required this.uses,
    required this.maxUses,
    required this.expiresAt,
    required this.revokedAt,
    required this.createdAt,
  });

  final String inviteId;
  final String role;
  final int uses;
  final int maxUses;
  final DateTime? expiresAt;
  final DateTime? revokedAt;
  final DateTime createdAt;

  factory InviteSummaryDto.fromJson(Map<String, dynamic> json) {
    return InviteSummaryDto(
      inviteId: json['inviteId'] as String,
      role: json['role'] as String,
      uses: (json['uses'] as num).toInt(),
      maxUses: (json['maxUses'] as num).toInt(),
      expiresAt: _parseDateTime(json['expiresAt']),
      revokedAt: _parseDateTime(json['revokedAt']),
      createdAt: _parseDateTime(json['createdAt']) ?? DateTime.fromMillisecondsSinceEpoch(0),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'inviteId': inviteId,
      'role': role,
      'uses': uses,
      'maxUses': maxUses,
      'expiresAt': expiresAt?.toIso8601String(),
      'revokedAt': revokedAt?.toIso8601String(),
      'createdAt': createdAt.toIso8601String(),
    };
  }

  static DateTime? _parseDateTime(Object? value) {
    if (value == null) {
      return null;
    }

    return DateTime.tryParse(value.toString())?.toLocal();
  }
}

class InviteSummaryPageDto {
  InviteSummaryPageDto({
    required this.items,
    required this.totalCount,
    required this.skip,
    required this.take,
  });

  final List<InviteSummaryDto> items;
  final int totalCount;
  final int skip;
  final int take;

  factory InviteSummaryPageDto.fromJson(Map<String, dynamic> json) {
    return InviteSummaryPageDto(
      items: _parseItems(json['items']),
      totalCount: (json['totalCount'] as num?)?.toInt() ?? 0,
      skip: (json['skip'] as num?)?.toInt() ?? 0,
      take: (json['take'] as num?)?.toInt() ?? 0,
    );
  }

  static List<InviteSummaryDto> _parseItems(Object? value) {
    if (value is! List) {
      return [];
    }

    return value
        .whereType<Map>()
        .map((item) => InviteSummaryDto.fromJson(item.map((key, data) => MapEntry(key.toString(), data))))
        .toList();
  }
}
