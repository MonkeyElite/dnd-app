import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/auth_models.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

final authControllerProvider = StateNotifierProvider<AuthController, AsyncValue<void>>(
  (ref) => AuthController(ref),
);

class AuthController extends StateNotifier<AsyncValue<void>> {
  AuthController(this._ref) : super(const AsyncData(null));

  final Ref _ref;

  Future<void> login({
    required String username,
    required String password,
  }) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final api = _ref.read(bffApiProvider);
      final response = await api.login(
        LoginRequestDto(username: username.trim(), password: password),
      );

      await _ref.read(sessionControllerProvider.notifier).setAuthenticated(
            token: response.accessToken,
            user: response.user,
          );
    });
  }

  Future<void> registerInvite({
    required String inviteCode,
    required String username,
    required String displayName,
    required String password,
  }) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      final api = _ref.read(bffApiProvider);
      final response = await api.registerWithInvite(
        RegisterInviteRequestDto(
          inviteCode: inviteCode.trim(),
          username: username.trim(),
          displayName: displayName.trim(),
          password: password,
        ),
      );

      await _ref.read(sessionControllerProvider.notifier).setAuthenticated(
            token: response.accessToken,
            user: response.user,
          );
    });
  }
}
