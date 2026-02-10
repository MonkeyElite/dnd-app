import 'package:dio/dio.dart';
import 'package:dnd_app/core/api/bff_api.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final dioProvider = Provider<Dio>((ref) {
  final session = ref.watch(sessionControllerProvider);
  final baseUrl = session.baseUrl ?? 'http://localhost:7000';

  final dio = Dio(
    BaseOptions(
      baseUrl: baseUrl,
      connectTimeout: const Duration(seconds: 10),
      receiveTimeout: const Duration(seconds: 20),
      sendTimeout: const Duration(seconds: 20),
      headers: const {'Accept': 'application/json'},
    ),
  );

  dio.interceptors.add(
    InterceptorsWrapper(
      onRequest: (options, handler) {
        final token = ref.read(sessionControllerProvider).token;
        if (token != null && token.isNotEmpty) {
          options.headers['Authorization'] = 'Bearer $token';
        }

        handler.next(options);
      },
    ),
  );

  return dio;
});

final bffApiProvider = Provider<BffApi>((ref) {
  return BffApi(
    ref.watch(dioProvider),
    onUnauthorized: () => ref.read(sessionControllerProvider.notifier).logout(),
  );
});
