using System.Net.Http.Json;
using DndApp.Bff.Contracts;

namespace DndApp.Bff.Clients;

public sealed class InventoryServiceClient(HttpClient httpClient, ILogger<InventoryServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetStorageLocationsAsync(
        Guid campaignId,
        Guid? placeId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var relativePath = $"/campaigns/{campaignId}/storage-locations";
        if (placeId.HasValue)
        {
            relativePath += $"?placeId={Uri.EscapeDataString(placeId.Value.ToString())}";
        }

        return SendAsync(HttpMethod.Get, relativePath, authorizationHeader, content: null, cancellationToken);
    }

    public Task<ForwardedJsonResponse> ForwardCreateStorageLocationAsync(
        Guid campaignId,
        InventoryCreateStorageLocationRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/storage-locations", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateLotAsync(
        Guid campaignId,
        InventoryCreateLotRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/inventory/lots", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateAdjustmentAsync(
        Guid campaignId,
        InventoryCreateAdjustmentRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/inventory/adjustments", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetSummaryAsync(
        Guid campaignId,
        Guid? placeId,
        Guid? storageLocationId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>();

        if (placeId.HasValue)
        {
            queryParts.Add($"placeId={Uri.EscapeDataString(placeId.Value.ToString())}");
        }

        if (storageLocationId.HasValue)
        {
            queryParts.Add($"storageLocationId={Uri.EscapeDataString(storageLocationId.Value.ToString())}");
        }

        var relativePath = $"/campaigns/{campaignId}/inventory/summary";
        if (queryParts.Count > 0)
        {
            relativePath += $"?{string.Join("&", queryParts)}";
        }

        return SendAsync(HttpMethod.Get, relativePath, authorizationHeader, content: null, cancellationToken);
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
