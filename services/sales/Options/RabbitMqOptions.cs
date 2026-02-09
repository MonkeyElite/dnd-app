namespace DndApp.Sales.Options;

public sealed class RabbitMqOptions
{
    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5672;

    public string User { get; init; } = "guest";

    public string Password { get; init; } = "guest";

    public string VirtualHost { get; init; } = "/";

    public string Exchange { get; init; } = "dndshop.events";

    public string InventoryQueue { get; init; } = "inventory.events";

    public int OutboxBatchSize { get; init; } = 50;

    public int PollDelayMilliseconds { get; init; } = 1500;
}
