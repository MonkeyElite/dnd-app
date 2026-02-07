namespace DndApp.Bff.Clients;

public sealed class MediaServiceClient(HttpClient httpClient, ILogger<MediaServiceClient> logger)
    : DownstreamServiceClientBase(httpClient, logger)
{
    public Task<bool> PingAsync(CancellationToken cancellationToken = default)
        => PingReadyAsync(cancellationToken);
}
