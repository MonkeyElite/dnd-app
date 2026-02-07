using DndApp.Bff.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DndApp.Bff.Controllers;

[ApiController]
[Route("api/v1/health")]
public sealed class HealthController : ControllerBase
{
    private readonly IdentityServiceClient _identityServiceClient;
    private readonly CampaignServiceClient _campaignServiceClient;
    private readonly CatalogServiceClient _catalogServiceClient;
    private readonly InventoryServiceClient _inventoryServiceClient;
    private readonly SalesServiceClient _salesServiceClient;
    private readonly MediaServiceClient _mediaServiceClient;

    public HealthController(
        IdentityServiceClient identityServiceClient,
        CampaignServiceClient campaignServiceClient,
        CatalogServiceClient catalogServiceClient,
        InventoryServiceClient inventoryServiceClient,
        SalesServiceClient salesServiceClient,
        MediaServiceClient mediaServiceClient)
    {
        _identityServiceClient = identityServiceClient;
        _campaignServiceClient = campaignServiceClient;
        _catalogServiceClient = catalogServiceClient;
        _inventoryServiceClient = inventoryServiceClient;
        _salesServiceClient = salesServiceClient;
        _mediaServiceClient = mediaServiceClient;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        var identityPingTask = _identityServiceClient.PingAsync(cancellationToken);
        var campaignPingTask = _campaignServiceClient.PingAsync(cancellationToken);
        var catalogPingTask = _catalogServiceClient.PingAsync(cancellationToken);
        var inventoryPingTask = _inventoryServiceClient.PingAsync(cancellationToken);
        var salesPingTask = _salesServiceClient.PingAsync(cancellationToken);
        var mediaPingTask = _mediaServiceClient.PingAsync(cancellationToken);

        await Task.WhenAll(
            identityPingTask,
            campaignPingTask,
            catalogPingTask,
            inventoryPingTask,
            salesPingTask,
            mediaPingTask);

        var response = new Dictionary<string, string>
        {
            ["bff"] = "ok",
            ["identity"] = identityPingTask.Result ? "ok" : "down",
            ["campaign"] = campaignPingTask.Result ? "ok" : "down",
            ["catalog"] = catalogPingTask.Result ? "ok" : "down",
            ["inventory"] = inventoryPingTask.Result ? "ok" : "down",
            ["sales"] = salesPingTask.Result ? "ok" : "down",
            ["media"] = mediaPingTask.Result ? "ok" : "down"
        };

        return Ok(response);
    }
}
