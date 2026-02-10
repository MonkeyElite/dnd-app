import 'package:dnd_app/core/api/dio_provider.dart';
import 'package:dnd_app/core/api/models/sales_models.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class SalesDraftArgs {
  const SalesDraftArgs({
    required this.campaignId,
    required this.draftId,
  });

  final String campaignId;
  final String draftId;

  @override
  bool operator ==(Object other) {
    return other is SalesDraftArgs && campaignId == other.campaignId && draftId == other.draftId;
  }

  @override
  int get hashCode => Object.hash(campaignId, draftId);
}

class SalesReceiptArgs {
  const SalesReceiptArgs({
    required this.campaignId,
    required this.saleId,
  });

  final String campaignId;
  final String saleId;

  @override
  bool operator ==(Object other) {
    return other is SalesReceiptArgs && campaignId == other.campaignId && saleId == other.saleId;
  }

  @override
  int get hashCode => Object.hash(campaignId, saleId);
}

final salesPageProvider = FutureProvider.family<SalesPageDto, String>((ref, campaignId) async {
  return ref.read(bffApiProvider).getSalesPage(campaignId);
});

final salesDraftPageProvider = FutureProvider.family<SalesDraftPageDto, SalesDraftArgs>((ref, args) async {
  return ref.read(bffApiProvider).getSalesDraftPage(args.campaignId, args.draftId);
});

final salesReceiptPageProvider =
    FutureProvider.family<SalesReceiptPageDto, SalesReceiptArgs>((ref, args) async {
      return ref.read(bffApiProvider).getSalesReceiptPage(args.campaignId, args.saleId);
    });

final salesDraftControllerProvider =
    StateNotifierProvider<SalesDraftController, AsyncValue<void>>((ref) {
      return SalesDraftController(ref);
    });

class SalesDraftController extends StateNotifier<AsyncValue<void>> {
  SalesDraftController(this._ref) : super(const AsyncData(null));

  final Ref _ref;

  Future<String> createDraft(String campaignId) async {
    state = const AsyncLoading();
    try {
      final api = _ref.read(bffApiProvider);
      final home = await api.getCampaignHomePage(campaignId);
      final locations = await api.getInventoryLocationsPage(campaignId);

      if (locations.locations.isEmpty) {
        throw const AppException(
          type: AppExceptionType.validation,
          message: 'Create at least one storage location before creating a sale draft.',
        );
      }

      final response = await api.createDraftSale(
        SalesDraftCreateRequestDto(
          campaignId: campaignId,
          soldWorldDay: home.currentWorldDay,
          storageLocationId: locations.locations.first.storageLocationId,
          customerId: null,
          notes: null,
        ),
      );

      _ref.invalidate(salesPageProvider(campaignId));
      state = const AsyncData(null);
      return response.draftId;
    } catch (error, stackTrace) {
      state = AsyncError(error, stackTrace);
      rethrow;
    }
  }

  Future<void> addLine(SalesDraftAddLineRequestDto request) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      await _ref.read(bffApiProvider).addDraftSaleLine(request);
      _invalidateDraft(request.campaignId, request.draftId);
    });
  }

  Future<void> updateLine(SalesDraftUpdateLineRequestDto request) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      await _ref.read(bffApiProvider).updateDraftSaleLine(request);
      _invalidateDraft(request.campaignId, request.draftId);
    });
  }

  Future<void> removeLine(SalesDraftRemoveLineRequestDto request) async {
    state = const AsyncLoading();
    state = await AsyncValue.guard(() async {
      await _ref.read(bffApiProvider).removeDraftSaleLine(request);
      _invalidateDraft(request.campaignId, request.draftId);
    });
  }

  Future<String> completeDraft(SalesDraftCompleteRequestDto request) async {
    state = const AsyncLoading();
    try {
      final response = await _ref.read(bffApiProvider).completeDraftSale(request);
      _invalidateDraft(request.campaignId, request.draftId);
      _ref.invalidate(salesReceiptPageProvider(
        SalesReceiptArgs(campaignId: request.campaignId, saleId: request.draftId),
      ));
      state = const AsyncData(null);
      return response.saleId;
    } catch (error, stackTrace) {
      state = AsyncError(error, stackTrace);
      rethrow;
    }
  }

  void _invalidateDraft(String campaignId, String draftId) {
    _ref.invalidate(salesPageProvider(campaignId));
    _ref.invalidate(salesDraftPageProvider(SalesDraftArgs(campaignId: campaignId, draftId: draftId)));
  }
}
