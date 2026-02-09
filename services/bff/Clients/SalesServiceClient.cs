using System.Net.Http.Json;
using DndApp.Bff.Contracts;

namespace DndApp.Bff.Clients;

public sealed class SalesServiceClient(HttpClient httpClient, ILogger<SalesServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateSaleAsync(
        Guid campaignId,
        SalesCreateRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/sales", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardUpdateSaleAsync(
        Guid campaignId,
        Guid saleId,
        SalesUpdateRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPutAsJsonAsync($"/campaigns/{campaignId}/sales/{saleId}", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCompleteSaleAsync(
        Guid campaignId,
        Guid saleId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync(
            $"/campaigns/{campaignId}/sales/{saleId}/complete",
            new SalesCompleteRequest(),
            authorizationHeader,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardVoidSaleAsync(
        Guid campaignId,
        Guid saleId,
        SalesVoidRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync(
            $"/campaigns/{campaignId}/sales/{saleId}/void",
            request,
            authorizationHeader,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetSalesAsync(
        Guid campaignId,
        int? fromWorldDay,
        int? toWorldDay,
        Guid? customerId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>();

        if (fromWorldDay.HasValue)
        {
            queryParts.Add($"fromWorldDay={Uri.EscapeDataString(fromWorldDay.Value.ToString())}");
        }

        if (toWorldDay.HasValue)
        {
            queryParts.Add($"toWorldDay={Uri.EscapeDataString(toWorldDay.Value.ToString())}");
        }

        if (customerId.HasValue)
        {
            queryParts.Add($"customerId={Uri.EscapeDataString(customerId.Value.ToString())}");
        }

        var relativePath = $"/campaigns/{campaignId}/sales";
        if (queryParts.Count > 0)
        {
            relativePath += $"?{string.Join("&", queryParts)}";
        }

        return SendAsync(HttpMethod.Get, relativePath, authorizationHeader, content: null, cancellationToken);
    }

    public Task<ForwardedJsonResponse> ForwardGetSaleAsync(
        Guid campaignId,
        Guid saleId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, $"/campaigns/{campaignId}/sales/{saleId}", authorizationHeader, content: null, cancellationToken);

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
