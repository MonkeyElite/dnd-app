using System.Net.Http.Json;
using DndApp.Bff.Contracts;

namespace DndApp.Bff.Clients;

public sealed class IdentityServiceClient(HttpClient httpClient, ILogger<IdentityServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);

    public Task<ForwardedJsonResponse> ForwardLoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync("/auth/login", request, authorizationHeader: null, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardRegisterWithInviteAsync(
        RegisterWithInviteRequest request,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync("/auth/register-with-invite", request, authorizationHeader: null, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardCreateInviteAsync(
        Guid campaignId,
        IdentityCreateInviteRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync($"/campaigns/{campaignId}/invites", request, authorizationHeader, cancellationToken);

    private async Task<ForwardedJsonResponse> SendPostAsJsonAsync<TRequest>(
        string relativePath,
        TRequest payload,
        string? authorizationHeader,
        CancellationToken cancellationToken)
    {
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, relativePath)
        {
            Content = JsonContent.Create(payload)
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
