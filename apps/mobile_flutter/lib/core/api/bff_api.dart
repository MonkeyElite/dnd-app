import 'package:dio/dio.dart';
import 'package:dnd_app/core/api/models/models.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/errors/error_mapper.dart';

class BffApi {
  BffApi(this._dio, {required Future<void> Function() onUnauthorized})
    : _onUnauthorized = onUnauthorized;

  final Dio _dio;
  final Future<void> Function() _onUnauthorized;

  Future<Map<String, String>> testConnection() async {
    return _request(() => _dio.get('/api/v1/health'), (data) {
      final map = _asMap(data);
      return map.map((key, value) => MapEntry(key, value.toString()));
    });
  }

  Future<AuthResponseDto> login(LoginRequestDto request) async {
    return _request(
      () => _dio.post('/api/v1/actions/auth/login', data: request.toJson()),
      (data) => AuthResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<AuthResponseDto> registerWithInvite(
    RegisterInviteRequestDto request,
  ) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/auth/register-invite',
        data: request.toJson(),
      ),
      (data) => AuthResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<CampaignsPageDto> getCampaignsPage() async {
    return _request(
      () => _dio.get('/api/v1/pages/campaigns'),
      (data) => CampaignsPageDto.fromJson(_asMap(data)),
    );
  }

  Future<String> createCampaign({
    required String name,
    String? description,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/pages/campaigns',
        data: {'name': name, 'description': description},
      ),
      (data) {
        final map = _asMap(data);
        return map['campaignId']?.toString() ?? '';
      },
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
          if (search != null && search.trim().isNotEmpty)
            'search': search.trim(),
          if (categoryId != null && categoryId.isNotEmpty)
            'categoryId': categoryId,
          'archived': 'IncludeArchived',
        },
      ),
      (data) => CatalogPageDto.fromJson(_asMap(data)),
    );
  }

  Future<CatalogItemPageDto> getCatalogItemPage(
    String campaignId,
    String itemId,
  ) async {
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
          if (storageLocationId != null && storageLocationId.isNotEmpty)
            'storageLocationId': storageLocationId,
          if (search != null && search.trim().isNotEmpty)
            'search': search.trim(),
        },
      ),
      (data) => InventoryPageDto.fromJson(_asMap(data)),
    );
  }

  Future<InventoryLocationsPageDto> getInventoryLocationsPage(
    String campaignId,
  ) async {
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
      () => _dio.get(
        '/api/v1/pages/campaign/$campaignId/inventory/location/$locationId',
      ),
      (data) => InventoryLocationDetailPageDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesPageDto> getSalesPage(String campaignId) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/sales'),
      (data) => SalesPageDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftPageDto> getSalesDraftPage(
    String campaignId,
    String draftId,
  ) async {
    return _request(
      () => _dio.get('/api/v1/pages/campaign/$campaignId/sales/draft/$draftId'),
      (data) => SalesDraftPageDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesReceiptPageDto> getSalesReceiptPage(
    String campaignId,
    String saleId,
  ) async {
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

  Future<CreateInviteResultDto> createInvite({
    required String campaignId,
    required String role,
    required int maxUses,
    int? expiresInDays,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/invites',
        data: {
          'campaignId': campaignId,
          'role': role,
          'maxUses': maxUses,
          'expiresInDays': expiresInDays,
        },
      ),
      (data) {
        final map = _asMap(data);
        return CreateInviteResultDto.fromJson(map);
      },
    );
  }

  Future<InviteSummaryPageDto> getInvitesPage(
    String campaignId, {
    required int skip,
    required int take,
  }) async {
    return _request(
      () => _dio.get(
        '/api/v1/actions/invites',
        queryParameters: {'campaignId': campaignId, 'skip': skip, 'take': take},
      ),
      (data) => InviteSummaryPageDto.fromJson(_asMap(data)),
    );
  }

  Future<bool> revokeInvite({
    required String campaignId,
    required String inviteId,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/invites/$inviteId/revoke',
        data: {'campaignId': campaignId},
      ),
      (data) {
        final map = _asMap(data);
        return map['revoked'] == true;
      },
    );
  }

  Future<void> updateMemberRole({
    required String campaignId,
    required String userId,
    required String role,
  }) async {
    await _request<bool>(
      () => _dio.put(
        '/api/v1/actions/members/$userId/role',
        data: {'campaignId': campaignId, 'role': role},
      ),
      (_) => true,
    );
  }

  Future<bool> updateCampaignCalendar({
    required String campaignId,
    required CalendarConfigDto calendar,
  }) async {
    return _request(
      () => _dio.put(
        '/api/v1/actions/campaign-settings/calendar',
        data: {'campaignId': campaignId, 'calendar': calendar.toJson()},
      ),
      (data) {
        final map = _asMap(data);
        return map['updated'] == true;
      },
    );
  }

  Future<bool> updateCampaignCurrency({
    required String campaignId,
    required CurrencyConfigDto currency,
  }) async {
    return _request(
      () => _dio.put(
        '/api/v1/actions/campaign-settings/currency',
        data: {'campaignId': campaignId, 'currency': currency.toJson()},
      ),
      (data) {
        final map = _asMap(data);
        return map['updated'] == true;
      },
    );
  }

  Future<String> createCategory({
    required String campaignId,
    required String name,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/categories',
        data: {'campaignId': campaignId, 'name': name},
      ),
      (data) {
        final map = _asMap(data);
        return map['categoryId']?.toString() ?? '';
      },
    );
  }

  Future<String> createUnit({
    required String campaignId,
    required String name,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/units',
        data: {'campaignId': campaignId, 'name': name},
      ),
      (data) {
        final map = _asMap(data);
        return map['unitId']?.toString() ?? '';
      },
    );
  }

  Future<String> createTag({
    required String campaignId,
    required String name,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/tags',
        data: {'campaignId': campaignId, 'name': name},
      ),
      (data) {
        final map = _asMap(data);
        return map['tagId']?.toString() ?? '';
      },
    );
  }

  Future<String> createCatalogItem({
    required String campaignId,
    required String name,
    required String categoryId,
    required String unitId,
    required int baseValueMinor,
    int? defaultListPriceMinor,
    String? description,
    double? weight,
    String? imageAssetId,
    List<String>? tagIds,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/items',
        data: {
          'campaignId': campaignId,
          'name': name,
          'description': description,
          'categoryId': categoryId,
          'unitId': unitId,
          'baseValueMinor': baseValueMinor,
          'defaultListPriceMinor': defaultListPriceMinor,
          'weight': weight,
          'imageAssetId': imageAssetId,
          'tagIds': tagIds ?? <String>[],
        },
      ),
      (data) {
        final map = _asMap(data);
        return map['itemId']?.toString() ?? '';
      },
    );
  }

  Future<bool> updateCatalogItem({
    required String campaignId,
    required String itemId,
    required String name,
    required String categoryId,
    required String unitId,
    required int baseValueMinor,
    int? defaultListPriceMinor,
    String? description,
    double? weight,
    String? imageAssetId,
    List<String>? tagIds,
  }) async {
    return _request(
      () => _dio.put(
        '/api/v1/actions/items/$itemId',
        data: {
          'campaignId': campaignId,
          'name': name,
          'description': description,
          'categoryId': categoryId,
          'unitId': unitId,
          'baseValueMinor': baseValueMinor,
          'defaultListPriceMinor': defaultListPriceMinor,
          'weight': weight,
          'imageAssetId': imageAssetId,
          'tagIds': tagIds ?? <String>[],
        },
      ),
      (data) {
        final map = _asMap(data);
        return map['updated'] == true;
      },
    );
  }

  Future<bool> setCatalogItemArchived({
    required String campaignId,
    required String itemId,
    required bool isArchived,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/items/$itemId/archive',
        data: {'campaignId': campaignId, 'isArchived': isArchived},
      ),
      (data) {
        final map = _asMap(data);
        return map['updated'] == true;
      },
    );
  }

  Future<String> createStorageLocation({
    required String campaignId,
    required String name,
    required String type,
    String? placeId,
    String? code,
    String? notes,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/storage-locations',
        data: {
          'campaignId': campaignId,
          'placeId': placeId,
          'name': name,
          'code': code,
          'type': type,
          'notes': notes,
        },
      ),
      (data) {
        final map = _asMap(data);
        return map['storageLocationId']?.toString() ?? '';
      },
    );
  }

  Future<String> createInventoryLot({
    required String campaignId,
    required String itemId,
    required String storageLocationId,
    required double quantity,
    required int unitCostMinor,
    required int acquiredWorldDay,
    String? source,
    String? notes,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/inventory/lots',
        data: {
          'campaignId': campaignId,
          'itemId': itemId,
          'storageLocationId': storageLocationId,
          'quantity': quantity,
          'unitCostMinor': unitCostMinor,
          'acquiredWorldDay': acquiredWorldDay,
          'source': source,
          'notes': notes,
        },
      ),
      (data) {
        final map = _asMap(data);
        return map['lotId']?.toString() ?? '';
      },
    );
  }

  Future<String> createInventoryAdjustment({
    required String campaignId,
    required String itemId,
    required String storageLocationId,
    required double deltaQuantity,
    required String reason,
    required int worldDay,
    String? lotId,
    String? notes,
    String? referenceType,
    String? referenceId,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/inventory/adjustments',
        data: {
          'campaignId': campaignId,
          'itemId': itemId,
          'storageLocationId': storageLocationId,
          'lotId': lotId,
          'deltaQuantity': deltaQuantity,
          'reason': reason,
          'worldDay': worldDay,
          'notes': notes,
          'referenceType': referenceType,
          'referenceId': referenceId,
        },
      ),
      (data) {
        final map = _asMap(data);
        return map['adjustmentId']?.toString() ?? '';
      },
    );
  }

  Future<SalesDraftCreateResponseDto> createDraftSale(
    SalesDraftCreateRequestDto request,
  ) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/sales/draft/create',
        data: request.toJson(),
      ),
      (data) => SalesDraftCreateResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesCreateResponseDto> createSale(
    SalesCreateActionRequestDto request,
  ) async {
    return _request(
      () => _dio.post('/api/v1/actions/sales', data: request.toJson()),
      (data) => SalesCreateResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<void> updateSale({
    required String saleId,
    required SalesUpdateActionRequestDto request,
  }) async {
    await _request<bool>(
      () => _dio.put('/api/v1/actions/sales/$saleId', data: request.toJson()),
      (_) => true,
    );
  }

  Future<String> completeSale({
    required String saleId,
    required SalesCompleteActionRequestDto request,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/sales/$saleId/complete',
        data: request.toJson(),
      ),
      (data) {
        final map = _asMap(data);
        return map['status']?.toString() ?? '';
      },
    );
  }

  Future<SalesDraftMutationResponseDto> addDraftSaleLine(
    SalesDraftAddLineRequestDto request,
  ) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/sales/draft/add-line',
        data: request.toJson(),
      ),
      (data) => SalesDraftMutationResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftMutationResponseDto> updateDraftSaleLine(
    SalesDraftUpdateLineRequestDto request,
  ) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/sales/draft/update-line',
        data: request.toJson(),
      ),
      (data) => SalesDraftMutationResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftMutationResponseDto> removeDraftSaleLine(
    SalesDraftRemoveLineRequestDto request,
  ) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/sales/draft/remove-line',
        data: request.toJson(),
      ),
      (data) => SalesDraftMutationResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<SalesDraftCompleteResponseDto> completeDraftSale(
    SalesDraftCompleteRequestDto request,
  ) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/sales/draft/complete',
        data: request.toJson(),
      ),
      (data) => SalesDraftCompleteResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<String> voidSale({
    required String campaignId,
    required String saleId,
    required String reason,
  }) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/sales/$saleId/void',
        data: {'campaignId': campaignId, 'reason': reason},
      ),
      (data) {
        final map = _asMap(data);
        return map['status']?.toString() ?? '';
      },
    );
  }

  Future<CreateUploadResponseDto> requestUpload(
    RequestUploadActionDto request,
  ) async {
    return _request(
      () => _dio.post(
        '/api/v1/actions/media/request-upload',
        data: request.toJson(),
      ),
      (data) => CreateUploadResponseDto.fromJson(_asMap(data)),
    );
  }

  Future<void> confirmUpload(ConfirmUploadActionDto request) async {
    await _request<bool>(
      () => _dio.post(
        '/api/v1/actions/media/confirm-upload',
        data: request.toJson(),
      ),
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
      throw ErrorMapper.mapDioException(
        exception,
        onUnauthorized: _onUnauthorized,
      );
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
