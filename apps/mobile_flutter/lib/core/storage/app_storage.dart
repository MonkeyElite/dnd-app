import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:shared_preferences/shared_preferences.dart';

class AppStorage {
  AppStorage({
    required FlutterSecureStorage secureStorage,
    required SharedPreferences sharedPreferences,
  })  : _secureStorage = secureStorage,
        _sharedPreferences = sharedPreferences;

  static const _tokenKey = 'auth_token';
  static const _baseUrlKey = 'base_url';
  static const _lastCampaignIdKey = 'last_campaign_id';

  final FlutterSecureStorage _secureStorage;
  final SharedPreferences _sharedPreferences;

  Future<String?> readToken() => _secureStorage.read(key: _tokenKey);

  Future<void> saveToken(String token) => _secureStorage.write(key: _tokenKey, value: token);

  Future<void> clearToken() => _secureStorage.delete(key: _tokenKey);

  String? readBaseUrl() => _sharedPreferences.getString(_baseUrlKey);

  Future<void> saveBaseUrl(String value) => _sharedPreferences.setString(_baseUrlKey, value);

  String? readLastCampaignId() => _sharedPreferences.getString(_lastCampaignIdKey);

  Future<void> saveLastCampaignId(String value) => _sharedPreferences.setString(_lastCampaignIdKey, value);

  Future<void> clearLastCampaignId() => _sharedPreferences.remove(_lastCampaignIdKey);
}
