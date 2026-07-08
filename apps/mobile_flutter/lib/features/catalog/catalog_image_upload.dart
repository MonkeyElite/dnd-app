import 'package:dio/dio.dart';
import 'package:dnd_app/core/api/bff_api.dart';
import 'package:dnd_app/core/api/models/media_models.dart';
import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:image_picker/image_picker.dart';

class CatalogImageUploadService {
  CatalogImageUploadService(this._api, {Dio? uploadDio})
    : _uploadDio = uploadDio ?? Dio();

  final BffApi _api;
  final Dio _uploadDio;

  static String resolveContentType(XFile file) {
    final mimeType = file.mimeType?.toLowerCase().trim();
    if (mimeType == 'image/jpg' || mimeType == 'image/pjpeg') {
      return 'image/jpeg';
    }

    if (_supportedContentTypes.contains(mimeType)) {
      return mimeType!;
    }

    final lowerPath = file.path.toLowerCase();
    final lowerName = file.name.toLowerCase();
    if (_hasAnyExtension(lowerPath, const ['.jpg', '.jpeg']) ||
        _hasAnyExtension(lowerName, const ['.jpg', '.jpeg'])) {
      return 'image/jpeg';
    }

    if (_hasAnyExtension(lowerPath, const ['.png']) ||
        _hasAnyExtension(lowerName, const ['.png'])) {
      return 'image/png';
    }

    if (_hasAnyExtension(lowerPath, const ['.webp']) ||
        _hasAnyExtension(lowerName, const ['.webp'])) {
      return 'image/webp';
    }

    throw const AppException(
      type: AppExceptionType.validation,
      message: 'Please choose a JPEG, PNG, or WebP image.',
    );
  }

  Future<String> uploadCatalogItemImage({
    required String campaignId,
    required XFile file,
  }) async {
    final contentType = resolveContentType(file);
    final bytes = await file.readAsBytes();
    final fileName = _resolveFileName(file);

    final upload = await _api.requestUpload(
      RequestUploadActionDto(
        campaignId: campaignId,
        purpose: 'catalog-item-image',
        fileName: fileName,
        contentType: contentType,
        sizeBytes: bytes.length,
      ),
    );

    try {
      await _uploadDio.put<void>(
        upload.uploadUrl,
        data: bytes,
        options: Options(
          headers: {
            Headers.contentTypeHeader: contentType,
            Headers.contentLengthHeader: bytes.length,
          },
          responseType: ResponseType.plain,
        ),
      );
    } on DioException catch (exception) {
      final statusCode = exception.response?.statusCode;
      throw AppException(
        type: AppExceptionType.server,
        statusCode: statusCode,
        message: 'Unable to upload image.',
      );
    }

    await _api.confirmUpload(
      ConfirmUploadActionDto(
        campaignId: campaignId,
        assetId: upload.assetId,
        sha256: null,
        sizeBytes: bytes.length,
      ),
    );

    return upload.assetId;
  }

  static String _resolveFileName(XFile file) {
    final name = file.name.trim();
    if (name.isNotEmpty) {
      return name;
    }

    final pathParts = file.path.split(RegExp(r'[\\/]'));
    final fallback = pathParts.isEmpty ? '' : pathParts.last.trim();
    return fallback.isEmpty ? 'catalog-item-image' : fallback;
  }

  static bool _hasAnyExtension(String value, List<String> extensions) {
    return extensions.any(value.endsWith);
  }

  static const _supportedContentTypes = {
    'image/jpeg',
    'image/png',
    'image/webp',
  };
}
