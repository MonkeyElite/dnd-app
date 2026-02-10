enum AppExceptionType {
  network,
  unauthorized,
  forbidden,
  validation,
  server,
  unknown,
}

class AppException implements Exception {
  const AppException({
    required this.type,
    required this.message,
    this.fieldErrors = const {},
    this.statusCode,
  });

  final AppExceptionType type;
  final String message;
  final Map<String, String> fieldErrors;
  final int? statusCode;

  @override
  String toString() => message;
}
