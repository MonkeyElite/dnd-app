using DndApp.Bff.Clients;

namespace DndApp.Bff.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDownstreamServiceClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient<IdentityServiceClient>(client =>
        {
            client.BaseAddress = GetRequiredBaseAddress(configuration, "Services:Identity:BaseUrl");
        });

        services.AddHttpClient<CampaignServiceClient>(client =>
        {
            client.BaseAddress = GetRequiredBaseAddress(configuration, "Services:Campaign:BaseUrl");
        });

        services.AddHttpClient<CatalogServiceClient>(client =>
        {
            client.BaseAddress = GetRequiredBaseAddress(configuration, "Services:Catalog:BaseUrl");
        });

        services.AddHttpClient<InventoryServiceClient>(client =>
        {
            client.BaseAddress = GetRequiredBaseAddress(configuration, "Services:Inventory:BaseUrl");
        });

        services.AddHttpClient<SalesServiceClient>(client =>
        {
            client.BaseAddress = GetRequiredBaseAddress(configuration, "Services:Sales:BaseUrl");
        });

        services.AddHttpClient<MediaServiceClient>(client =>
        {
            client.BaseAddress = GetRequiredBaseAddress(configuration, "Services:Media:BaseUrl");
        });

        return services;
    }

    private static Uri GetRequiredBaseAddress(IConfiguration configuration, string key)
    {
        var value = configuration[key];

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            throw new InvalidOperationException($"Configuration value '{key}' must be a valid absolute URI.");
        }

        return uri;
    }
}
