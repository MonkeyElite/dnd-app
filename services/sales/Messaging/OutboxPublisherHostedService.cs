using System.Text;
using DndApp.Sales.Data;
using DndApp.Sales.Options;
using DndShop.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DndApp.Sales.Messaging;

public sealed class OutboxPublisherHostedService(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    ILogger<OutboxPublisherHostedService> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = rabbitMqOptions.Value;
    private readonly ILogger<OutboxPublisherHostedService> _logger = logger;

    private IConnection? _connection;
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PublishPendingOutboxMessagesAsync(serviceProvider, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected outbox publisher failure.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_options.PollDelayMilliseconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Dispose();
        _channel = null;

        _connection?.Dispose();
        _connection = null;

        return base.StopAsync(cancellationToken);
    }

    private async Task PublishPendingOutboxMessagesAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        EnsureBrokerTopologyDeclared();

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SalesDbContext>();

        var outboxMessages = await dbContext.OutboxMessages
            .Where(x => x.PublishedAt == null)
            .OrderBy(x => x.OccurredAt)
            .Take(Math.Max(1, _options.OutboxBatchSize))
            .ToListAsync(cancellationToken);

        if (outboxMessages.Count == 0)
        {
            return;
        }

        foreach (var outboxMessage in outboxMessages)
        {
            try
            {
                Publish(outboxMessage);
                outboxMessage.PublishedAt = DateTimeOffset.UtcNow;
                outboxMessage.LastError = null;
            }
            catch (Exception exception)
            {
                outboxMessage.PublishAttempts++;
                outboxMessage.LastError = Truncate(exception.Message, 4000);

                _logger.LogError(
                    exception,
                    "Failed publishing outbox message {OutboxMessageId} (attempt {PublishAttempts}).",
                    outboxMessage.OutboxMessageId,
                    outboxMessage.PublishAttempts);

                await dbContext.SaveChangesAsync(cancellationToken);

                var retryDelaySeconds = Math.Min(30, Math.Pow(2, Math.Min(6, outboxMessage.PublishAttempts)));
                await Task.Delay(TimeSpan.FromSeconds(retryDelaySeconds), cancellationToken);
                continue;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private void Publish(Data.Entities.OutboxMessage outboxMessage)
    {
        EnsureBrokerTopologyDeclared();

        var payloadBytes = Encoding.UTF8.GetBytes(outboxMessage.PayloadJson);
        var properties = _channel!.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.Type = outboxMessage.Type;
        properties.Timestamp = new AmqpTimestamp(outboxMessage.OccurredAt.ToUnixTimeSeconds());
        properties.MessageId = outboxMessage.OutboxMessageId.ToString();
        properties.CorrelationId = outboxMessage.CorrelationId.ToString();

        _channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: outboxMessage.Type,
            mandatory: false,
            basicProperties: properties,
            body: payloadBytes);
    }

    private void EnsureBrokerTopologyDeclared()
    {
        if (_connection?.IsOpen != true)
        {
            _connection?.Dispose();

            var connectionFactory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.User,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            _connection = connectionFactory.CreateConnection();
            _channel?.Dispose();
            _channel = null;
        }

        if (_channel?.IsOpen == true)
        {
            return;
        }

        _channel?.Dispose();
        _channel = _connection!.CreateModel();
        _channel.ExchangeDeclare(
            exchange: _options.Exchange,
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false,
            arguments: null);
        _channel.QueueDeclare(
            queue: _options.InventoryQueue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
        _channel.QueueBind(
            queue: _options.InventoryQueue,
            exchange: _options.Exchange,
            routingKey: SalesEventTypes.SaleCompletedV1);
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
