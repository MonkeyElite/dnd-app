import 'dart:async';

import 'package:dnd_app/core/api/models/auth_models.dart';
import 'package:dnd_app/core/storage/app_storage.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

final secureStorageProvider = Provider<FlutterSecureStorage>(
  (ref) => const FlutterSecureStorage(),
);

final sharedPreferencesProvider = FutureProvider<SharedPreferences>(
  (ref) => SharedPreferences.getInstance(),
);

final appStorageProvider = FutureProvider<AppStorage>((ref) async {
  final prefs = await ref.watch(sharedPreferencesProvider.future);
  return AppStorage(
    secureStorage: ref.watch(secureStorageProvider),
    sharedPreferences: prefs,
  );
});

final sessionControllerProvider =
    StateNotifierProvider<SessionController, SessionState>((ref) {
      final controller = SessionController(ref);
      unawaited(controller.initialize());
      return controller;
    });

class SessionState {
  const SessionState({
    this.initialized = false,
    this.baseUrl,
    this.token,
    this.user,
    this.selectedCampaignId,
  });

  final bool initialized;
  final String? baseUrl;
  final String? token;
  final AuthUserDto? user;
  final String? selectedCampaignId;

  bool get isAuthenticated => token != null && token!.isNotEmpty;

  SessionState copyWith({
    bool? initialized,
    String? baseUrl,
    String? token,
    AuthUserDto? user,
    String? selectedCampaignId,
    bool clearUser = false,
    bool clearToken = false,
    bool clearSelectedCampaignId = false,
  }) {
    return SessionState(
      initialized: initialized ?? this.initialized,
      baseUrl: baseUrl ?? this.baseUrl,
      token: clearToken ? null : (token ?? this.token),
      user: clearUser ? null : (user ?? this.user),
      selectedCampaignId: clearSelectedCampaignId
          ? null
          : (selectedCampaignId ?? this.selectedCampaignId),
    );
  }
}

class SessionController extends StateNotifier<SessionState> {
  SessionController(this._ref) : super(const SessionState());

  final Ref _ref;

  Future<void> initialize() async {
    final storage = await _storage();
    final baseUrl = storage.readBaseUrl();
    final token = await storage.readToken();
    final selectedCampaignId = storage.readLastCampaignId();

    state = state.copyWith(
      initialized: true,
      baseUrl: baseUrl,
      token: token,
      selectedCampaignId: selectedCampaignId,
    );
  }

  Future<void> setBaseUrl(String baseUrl) async {
    final normalized = baseUrl.trim().replaceAll(RegExp(r'/$'), '');
    final storage = await _storage();
    await storage.saveBaseUrl(normalized);
    state = state.copyWith(baseUrl: normalized);
  }

  Future<void> setAuthenticated({
    required String token,
    required AuthUserDto user,
  }) async {
    final storage = await _storage();
    await storage.saveToken(token);

    state = state.copyWith(token: token, user: user);
  }

  Future<void> logout() async {
    final storage = await _storage();
    await storage.clearToken();

    state = state.copyWith(clearToken: true, clearUser: true);
  }

  Future<void> selectCampaign(String campaignId) async {
    final storage = await _storage();
    await storage.saveLastCampaignId(campaignId);

    state = state.copyWith(selectedCampaignId: campaignId);
  }

  Future<void> clearSelectedCampaign() async {
    final storage = await _storage();
    await storage.clearLastCampaignId();
    state = state.copyWith(clearSelectedCampaignId: true);
  }

  Future<AppStorage> _storage() => _ref.read(appStorageProvider.future);
}
