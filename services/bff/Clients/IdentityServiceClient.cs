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

    public Task<ForwardedJsonResponse> ForwardUpsertCampaignMembershipAsync(
        IdentityUpsertCampaignMembershipRequest request,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendPostAsJsonAsync("/campaign-memberships", request, authorizationHeader, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetMyCampaignMembershipsAsync(
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(HttpMethod.Get, "/me/campaign-memberships", authorizationHeader, content: null, cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetMyCampaignMembershipForCampaignAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/members/me",
            authorizationHeader,
            content: null,
            cancellationToken);

    public Task<ForwardedJsonResponse> ForwardGetCampaignMembersAsync(
        Guid campaignId,
        string? authorizationHeader,
        CancellationToken cancellationToken = default)
        => SendAsync(
            HttpMethod.Get,
            $"/campaigns/{campaignId}/members",
            authorizationHeader,
            content: null,
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
