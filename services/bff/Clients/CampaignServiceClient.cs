namespace DndApp.Bff.Clients;

public sealed class CampaignServiceClient(HttpClient httpClient, ILogger<CampaignServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);
}
