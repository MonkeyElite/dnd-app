import 'dart:async';

import 'package:dio/dio.dart';
import 'package:dnd_app/core/errors/app_exception.dart';

class ErrorMapper {
  static AppException mapDioException(
    DioException exception, {
    required Future<void> Function() onUnauthorized,
  }) {
    final response = exception.response;
    final statusCode = response?.statusCode;

    if (_isConnectivityIssue(exception)) {
      return const AppException(
        type: AppExceptionType.network,
        message: 'Unable to reach server. Check your connection and server URL.',
      );
    }

    final body = response?.data;
    final errorMessage = _extractMessage(body) ?? exception.message ?? 'Request failed.';
    final fieldErrors = _extractFieldErrors(body);

    if (statusCode == 401) {
      unawaited(onUnauthorized());
      return const AppException(
        type: AppExceptionType.unauthorized,
        message: 'Your session expired. Please sign in again.',
        statusCode: 401,
      );
    }

    if (statusCode == 403) {
      return const AppException(
        type: AppExceptionType.forbidden,
        message: 'You do not have permission to perform this action.',
        statusCode: 403,
      );
    }

    if (statusCode == 400 || statusCode == 422) {
      return AppException(
        type: AppExceptionType.validation,
        message: errorMessage,
        fieldErrors: fieldErrors,
        statusCode: statusCode,
      );
    }

    if (statusCode != null && statusCode >= 500) {
      return AppException(
        type: AppExceptionType.server,
        message: errorMessage,
        statusCode: statusCode,
      );
    }

    return AppException(
      type: AppExceptionType.unknown,
      message: errorMessage,
      statusCode: statusCode,
    );
  }

  static bool _isConnectivityIssue(DioException exception) {
    return exception.type == DioExceptionType.connectionError ||
        exception.type == DioExceptionType.connectionTimeout ||
        exception.type == DioExceptionType.receiveTimeout ||
        (exception.response == null && exception.type == DioExceptionType.unknown);
  }

  static String? _extractMessage(Object? body) {
    if (body is String && body.trim().isNotEmpty) {
      return body.trim();
    }

    if (body is Map<String, dynamic>) {
      final message = body['message'];
      if (message is String && message.trim().isNotEmpty) {
        return message.trim();
      }

      final title = body['title'];
      if (title is String && title.trim().isNotEmpty) {
        return title.trim();
      }
    }

    return null;
  }

  static Map<String, String> _extractFieldErrors(Object? body) {
    if (body is! Map<String, dynamic>) {
      return const {};
    }

    final errors = body['errors'];
    if (errors is! Map<String, dynamic>) {
      return const {};
    }

    final mapped = <String, String>{};
    for (final entry in errors.entries) {
      final value = entry.value;
      if (value is List && value.isNotEmpty) {
        mapped[entry.key] = value.first.toString();
      } else if (value is String && value.isNotEmpty) {
        mapped[entry.key] = value;
      }
    }

    return mapped;
  }
}
