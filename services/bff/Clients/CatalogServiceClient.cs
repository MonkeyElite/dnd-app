using System.Net.Http.Json;
using System.Text.Json;
using DndApp.Bff.Contracts;

namespace DndApp.Bff.Clients;

public sealed class CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetCategoriesAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/categories",
            authorizationHeader,
            content: null,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateCategoryAsync(
        Guid campaignId,
        CatalogCreateCategoryRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/categories", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetUnitsAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/units",
            authorizationHeader,
            content: null,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateUnitAsync(
        Guid campaignId,
        CatalogCreateUnitRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/units", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetTagsAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/tags",
            authorizationHeader,
            content: null,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateTagAsync(
        Guid campaignId,
        CatalogCreateTagRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/tags", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetItemsAsync(
        Guid campaignId,
        string? search,
        Guid? categoryId,
        string? archived,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            queryParts.Add($"search={Uri.EscapeDataString(search)}");
        }

        if (categoryId.HasValue)
        {
            queryParts.Add($"categoryId={Uri.EscapeDataString(categoryId.Value.ToString())}");
        }

        if (!string.IsNullOrWhiteSpace(archived))
        {
            queryParts.Add($"archived={Uri.EscapeDataString(archived)}");
        }

        var relativePath = $"/campaigns/{campaignId}/items";
        if (queryParts.Count > 0)
        {
            relativePath += $"?{string.Join("&", queryParts)}";
        }

        return SendAsync(HttpMethod.Get, relativePath, authorizationHeader, content: null, cancellationToken);
    }

    public Task<ForwardedJsonResponse> ForwardCreateItemAsync(
        Guid campaignId,
        CatalogCreateItemRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/items", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardUpdateItemAsync(
        Guid campaignId,
        Guid itemId,
        JsonElement requestBody,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPutAsJsonAsync($"/campaigns/{campaignId}/items/{itemId}", requestBody, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardSetItemArchiveStateAsync(
        Guid campaignId,
        Guid itemId,
        CatalogSetItemArchiveRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync(
            $"/campaigns/{campaignId}/items/{itemId}/archive",
            request,
            authorizationHeader,
            cancellationToken);

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

    private Task<ForwardedJsonResponse> SendPutAsJsonAsync<TRequest>(
        string relativePath,
        TRequest payload,
        string? authorizationHeader,
        CancellationToken cancellationToken)
        => SendAsync(
            HttpMethod.Put,
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
