namespace DndApp.Bff.Clients;

public sealed class InventoryServiceClient(HttpClient httpClient, ILogger<InventoryServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);
}
