namespace DndApp.Bff.Clients;

public sealed class SalesServiceClient(HttpClient httpClient, ILogger<SalesServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);
}
