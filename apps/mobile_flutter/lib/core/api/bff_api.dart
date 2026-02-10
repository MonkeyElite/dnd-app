import 'package:dio/dio.dart';
import 'package:dnd_app/core/api/models/models.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/errors/error_mapper.dart';

class BffApi {
  BffApi(
    this._dio, {
    required Future<void> Function() onUnauthorized,
  }) : _onUnauthorized = onUnauthorized;

  final Dio _dio;
  final Future<void> Function() _onUnauthorized;

  Future<Map<String, String>> testConnection() async {
    return _request(
      () => _dio.get('/api/v1/health'),
      (data) {
        final map = _asMap(data);
        return map.map((key, value) => MapEntry(key, value.toString()));
      },
    );
  }

  Future<AuthResponseDto> login(LoginRequestDto request) async {
    return _request(
      () => _dio.post('/api/v1/actions/auth/login', data: request.toJson()),
      (data) => AuthResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<AuthResponseDto> registerWithInvite(RegisterInviteRequestDto request) async {
    return _request(
      () => _dio.post('/api/v1/actions/auth/register-invite', data: request.toJson()),
      (data) => AuthResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<CampaignsPageDto> getCampaignsPage() async {
    return _request(
      () => _dio.get('/api/v1/pages/campaigns'),
      (data) => CampaignsPageDto.fromJson(_asMap(data)),
    );
  }

  Future<CampaignHomePageDto> getCampaignHomePage(String campaignId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/home'),
      (data) => CampaignHomePageDto.fromJson(_asMap(data)),
    );
  }

  Future<CatalogPageDto> getCatalogPage(
    String campaignId, {
    String? search,
    String? categoryId,
  }) async {
    return _request(
      () => _dio.get(
        '/api/v1/pages/campaign/$campaignId/catalog',
        queryParameters: {
          if (search != null && search.trim().isNotEmpty) 'search': search.trim(),
          if (categoryId != null && categoryId.isNotEmpty) 'categoryId': categoryId,
          'archived': 'IncludeArchived',
        },
      ),
      (data) => CatalogPageDto.fromJson(_asMap(data)),
    );
  }

  Future<CatalogItemPageDto> getCatalogItemPage(String campaignId, String itemId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/catalog/item/$itemId'),
      (data) => CatalogItemPageDto.fromJson(_asMap(data)),
    );
  }

  Future<InventoryPageDto> getInventorySummaryPage(
    String campaignId, {
    String? placeId,
    String? storageLocationId,
    String? search,
  }) async {
    return _request(
      () => _dio.get(
        '/api/v1/pages/campaign/$campaignId/inventory/summary',
        queryParameters: {
          if (placeId != null && placeId.isNotEmpty) 'placeId': placeId,
          if (storageLocationId != null && storageLocationId.isNotEmpty) 'storageLocationId': storageLocationId,
          if (search != null && search.trim().isNotEmpty) 'search': search.trim(),
        },
      ),
      (data) => InventoryPageDto.fromJson(_asMap(data)),
    );
  }

  Future<InventoryLocationsPageDto> getInventoryLocationsPage(String campaignId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/inventory/locations'),
      (data) => InventoryLocationsPageDto.fromJson(_asMap(data)),
    );
  }

  Future<InventoryLocationDetailPageDto> getInventoryLocationDetailPage(
    String campaignId,
    String locationId,
  ) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/inventory/location/$locationId'),
      (data) => InventoryLocationDetailPageDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesPageDto> getSalesPage(String campaignId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/sales'),
      (data) => SalesPageDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftPageDto> getSalesDraftPage(String campaignId, String draftId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/sales/draft/$draftId'),
      (data) => SalesDraftPageDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesReceiptPageDto> getSalesReceiptPage(String campaignId, String saleId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/sales/$saleId'),
      (data) => SalesReceiptPageDto.fromJson(_asMap(data)),
    );
  }

  Future<CampaignSettingsPageDto> getSettingsPage(String campaignId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/settings'),
      (data) => CampaignSettingsPageDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftCreateResponseDto> createDraftSale(SalesDraftCreateRequestDto request) async {
    return _request(
      () => _dio.post('/api/v1/actions/sales/draft/create', data: request.toJson()),
      (data) => SalesDraftCreateResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftMutationResponseDto> addDraftSaleLine(SalesDraftAddLineRequestDto request) async {
    return _request(
      () => _dio.post('/api/v1/actions/sales/draft/add-line', data: request.toJson()),
      (data) => SalesDraftMutationResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftMutationResponseDto> updateDraftSaleLine(
    SalesDraftUpdateLineRequestDto request,
  ) async {
    return _request(
      () => _dio.post('/api/v1/actions/sales/draft/update-line', data: request.toJson()),
      (data) => SalesDraftMutationResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftMutationResponseDto> removeDraftSaleLine(
    SalesDraftRemoveLineRequestDto request,
  ) async {
    return _request(
      () => _dio.post('/api/v1/actions/sales/draft/remove-line', data: request.toJson()),
      (data) => SalesDraftMutationResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftCompleteResponseDto> completeDraftSale(
    SalesDraftCompleteRequestDto request,
  ) async {
    return _request(
      () => _dio.post('/api/v1/actions/sales/draft/complete', data: request.toJson()),
      (data) => SalesDraftCompleteResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<CreateUploadResponseDto> requestUpload(RequestUploadActionDto request) async {
    return _request(
      () => _dio.post('/api/v1/actions/media/request-upload', data: request.toJson()),
      (data) => CreateUploadResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<void> confirmUpload(ConfirmUploadActionDto request) async {
    await _request<bool>(
      () => _dio.post('/api/v1/actions/media/confirm-upload', data: request.toJson()),
      (_) => true,
    );
  }

  Future<T> _request<T>(
    Future<Response<dynamic>> Function() call,
    T Function(Object? data) parse,
  ) async {
    try {
      final response = await call();
      return parse(response.data);
    } on DioException catch (exception) {
      throw ErrorMapper.mapDioException(exception, onUnauthorized: _onUnauthorized);
    } catch (_) {
      throw const AppException(
        type: AppExceptionType.unknown,
        message: 'Unexpected response from server.',
      );
    }
  }

  Map<String, dynamic> _asMap(Object? value) {
    if (value is Map<String, dynamic>) {
      return value;
    }

    if (value is Map) {
      return value.map((key, data) => MapEntry(key.toString(), data));
    }

    throw const AppException(
      type: AppExceptionType.unknown,
      message: 'Invalid payload format.',
    );
  }
}
