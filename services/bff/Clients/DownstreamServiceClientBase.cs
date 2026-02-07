namespace DndApp.Bff.Clients;

public abstract class DownstreamServiceClientBase
{
    private static readonly Uri ReadyEndpoint = new("/health/ready", UriKind.Relative);

    protected HttpClient HttpClient { get; }

    private readonly ILogger _logger;

    protected DownstreamServiceClientBase(HttpClient httpClient, ILogger logger)
    {
        HttpClient = httpClient;
        _logger = logger;
    }

    protected async Task<bool> PingReadyAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync(ReadyEndpoint, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Downstream health check failed for {BaseAddress}.", HttpClient.BaseAddress);
            return false;
        }
    }
}
