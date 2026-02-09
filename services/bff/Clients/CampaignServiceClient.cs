using System.Net.Http.Json;
using DndApp.Bff.Contracts;

namespace DndApp.Bff.Clients;

public sealed class CampaignServiceClient(HttpClient httpClient, ILogger<CampaignServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateCampaignAsync(
        CreateCampaignRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync("/campaigns", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetCampaignAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, $"/campaigns/{campaignId}", authorizationHeader, content: null, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetCalendarSettingsAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/settings/calendar",
            authorizationHeader,
            content: null,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardUpdateCalendarSettingsAsync(
        Guid campaignId,
        CampaignCalendarUpdateRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPutAsJsonAsync($"/campaigns/{campaignId}/settings/calendar", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetCurrencySettingsAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/settings/currency",
            authorizationHeader,
            content: null,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetPlacesAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/places",
            authorizationHeader,
            content: null,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetCustomersAsync(
        Guid campaignId,
        string? search,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
    {
        var relativePath = $"/campaigns/{campaignId}/customers";
        if (!string.IsNullOrWhiteSpace(search))
        {
            relativePath += $"?search={Uri.EscapeDataString(search.Trim())}";
        }

        return SendAsync(HttpMethod.Get, relativePath, authorizationHeader, content: null, cancellationToken);
    }

    public Task<ForwardedJsonResponse> ForwardUpdateCurrencySettingsAsync(
        Guid campaignId,
        CampaignCurrencyUpdateRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPutAsJsonAsync($"/campaigns/{campaignId}/settings/currency", request, authorizationHeader, cancellationToken);

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
