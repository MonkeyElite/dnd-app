using System.Net.Http.Json;
using DndApp.Bff.Contracts;

namespace DndApp.Bff.Clients;

public sealed class MediaServiceClient(HttpClient httpClient, ILogger<MediaServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateUploadAsync(
        Guid campaignId,
        MediaCreateAssetUploadRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync(
            $"/campaigns/{campaignId}/assets/uploads",
            request,
            authorizationHeader,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardFinalizeUploadAsync(
        Guid campaignId,
        Guid assetId,
        MediaFinalizeAssetUploadRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync(
            $"/campaigns/{campaignId}/assets/{assetId}/finalize",
            request,
            authorizationHeader,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetDownloadUrlAsync(
        Guid campaignId,
        Guid assetId,
        int? expiresInSeconds,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var relativePath = $"/campaigns/{campaignId}/assets/{assetId}/download-url";
        if (expiresInSeconds.HasValue)
        {
            relativePath += $"?expiresInSeconds={expiresInSeconds.Value}";
        }

        return SendAsync(HttpMethod.Get, relativePath, authorizationHeader, content: null, cancellationToken);
    }

    public async Task<HttpResponseMessage> ForwardDownloadAsync(
        Guid campaignId,
        Guid assetId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        using var httpRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/assets/{assetId}/download");

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        return await HttpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
    }

    private Task<ForwardedJsonResponse> SendPostAsJsonAsync<TRequest>(
        string relativePath,
        TRequest payload,
        string? authorizationHeader,
        CancellationToken cancellationToken)
        => SendAsync(
            HttpMethod.Post,
            relativePath,
            authorizationHeader,
            JsonContent.Create(payload),
            cancellationToken);

    private async Task<ForwardedJsonResponse> SendAsync(
        HttpMethod method,
        string relativePath,
        string? authorizationHeader,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(method, relativePath)
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            httpRequest.Headers.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        using var response = await HttpClient.SendAsync(httpRequest, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "application/json";

        return new ForwardedJsonResponse((int)response.StatusCode, contentType, body);
    }
}
