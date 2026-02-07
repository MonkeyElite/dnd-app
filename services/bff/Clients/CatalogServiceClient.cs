namespace DndApp.Bff.Clients;

public sealed class CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);
}
