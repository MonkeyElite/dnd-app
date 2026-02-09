using System.Text;
using System.Text.Json;
using DndApp.Inventory.Data;
using DndApp.Inventory.Data.Entities;
using DndApp.Inventory.Options;
using DndShop.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DndApp.Inventory.Messaging;

public sealed class SaleCompletedConsumerHostedService(
    IServiceProvider serviceProvider,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    ILogger<SaleCompletedConsumerHostedService> logger) : BackgroundService
{
    private readonly RabbitMqOptions _options = rabbitMqOptions.Value;
    private readonly ILogger<SaleCompletedConsumerHostedService> _logger = logger;

    private IConnection? _connection;
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            EnsureBrokerTopologyDeclared();

            var delivery = _channel!.BasicGet(_options.InventoryQueue, autoAck: false);
            if (delivery is null)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(_options.PollDelayMilliseconds), stoppingToken);
                continue;
            }

            try
            {
                var shouldAcknowledge = await HandleDeliveryAsync(delivery.Body, stoppingToken);
                if (shouldAcknowledge)
                {
                    _channel.BasicAck(delivery.DeliveryTag, multiple: false);
                }
                else
                {
                    _channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Inventory consumer failed to process a message.");
                _channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: true);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
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

    private async Task<bool> HandleDeliveryAsync(ReadOnlyMemory<byte> body, CancellationToken cancellationToken)
    {
        EventEnvelope<SaleCompletedEvent>? envelope;
        try
        {
            var messageJson = Encoding.UTF8.GetString(body.Span);
            envelope = JsonSerializer.Deserialize<EventEnvelope<SaleCompletedEvent>>(messageJson);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to deserialize inventory event payload.");
            return true;
        }

        if (envelope is null || envelope.Data is null)
        {
            _logger.LogWarning("Skipping empty inventory event payload.");
            return true;
        }

        if (!string.Equals(envelope.EventType, SalesEventTypes.SaleCompletedV1, StringComparison.Ordinal))
        {
            _logger.LogWarning("Skipping unsupported event type {EventType}.", envelope.EventType);
            return true;
        }

        try
        {
            await ApplySaleCompletedEventAsync(envelope, cancellationToken);
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process sale completed event {EventId}.",
                envelope.EventId);
            return false;
        }
    }

    private async Task ApplySaleCompletedEventAsync(
        EventEnvelope<SaleCompletedEvent> envelope,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var campaignId = envelope.CampaignId != Guid.Empty ? envelope.CampaignId : envelope.Data.CampaignId;
        var saleId = envelope.Data.SaleId;
        var storageLocationId = envelope.Data.StorageLocationId;
        var worldDay = Math.Max(0, envelope.Data.SoldWorldDay);

        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (await dbContext.ProcessedEvents.AnyAsync(x => x.EventId == envelope.EventId, cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        if (campaignId == Guid.Empty || saleId == Guid.Empty || storageLocationId == Guid.Empty)
        {
            _logger.LogError(
                "Ignoring malformed sale event {EventId}. campaignId={CampaignId}, saleId={SaleId}, storageLocationId={StorageLocationId}.",
                envelope.EventId,
                campaignId,
                saleId,
                storageLocationId);

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                EventId = envelope.EventId,
                ProcessedAt = now
            });

            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        foreach (var line in envelope.Data.Lines ?? [])
        {
            var quantityToConsume = NormalizeQuantity(line.Quantity);
            if (line.ItemId == Guid.Empty || quantityToConsume <= 0)
            {
                _logger.LogWarning(
                    "Skipping invalid sale line for event {EventId}. itemId={ItemId}, quantity={Quantity}.",
                    envelope.EventId,
                    line.ItemId,
                    line.Quantity);
                continue;
            }

            var lots = await dbContext.InventoryLots
                .Where(x => x.CampaignId == campaignId
                            && x.StorageLocationId == storageLocationId
                            && x.ItemId == line.ItemId)
                .OrderBy(x => x.AcquiredWorldDay)
                .ThenBy(x => x.CreatedAt)
                .ToListAsync(cancellationToken);

            var remaining = quantityToConsume;
            foreach (var lot in lots)
            {
                if (remaining <= 0)
                {
                    break;
                }

                if (lot.QuantityOnHand <= 0)
                {
                    continue;
                }

                var consumed = NormalizeQuantity(decimal.Min(lot.QuantityOnHand, remaining));
                if (consumed <= 0)
                {
                    continue;
                }

                lot.QuantityOnHand = NormalizeQuantity(lot.QuantityOnHand - consumed);
                lot.UpdatedAt = now;
                remaining = NormalizeQuantity(remaining - consumed);

                dbContext.InventoryAdjustments.Add(new InventoryAdjustment
                {
                    AdjustmentId = Guid.NewGuid(),
                    CampaignId = campaignId,
                    ItemId = line.ItemId,
                    StorageLocationId = storageLocationId,
                    LotId = lot.LotId,
                    DeltaQuantity = -consumed,
                    Reason = AdjustmentReason.Sale.ToString(),
                    WorldDay = worldDay,
                    Notes = null,
                    ReferenceType = "Sale",
                    ReferenceId = saleId,
                    CreatedByUserId = Guid.Empty,
                    CreatedAt = now
                });
            }

            if (remaining > 0)
            {
                _logger.LogError(
                    "INSUFFICIENT STOCK for sale {SaleId}, campaign {CampaignId}, item {ItemId}. Missing {MissingQuantity}. CorrelationId={CorrelationId}.",
                    saleId,
                    campaignId,
                    line.ItemId,
                    remaining,
                    envelope.CorrelationId);

                dbContext.InventoryAdjustments.Add(new InventoryAdjustment
                {
                    AdjustmentId = Guid.NewGuid(),
                    CampaignId = campaignId,
                    ItemId = line.ItemId,
                    StorageLocationId = storageLocationId,
                    LotId = null,
                    DeltaQuantity = -remaining,
                    Reason = AdjustmentReason.ManualCorrection.ToString(),
                    WorldDay = worldDay,
                    Notes = "INSUFFICIENT STOCK",
                    ReferenceType = "Sale",
                    ReferenceId = saleId,
                    CreatedByUserId = Guid.Empty,
                    CreatedAt = now
                });
            }
        }

        dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            EventId = envelope.EventId,
            ProcessedAt = now
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
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

    private static decimal NormalizeQuantity(decimal quantity)
        => decimal.Round(quantity, 3, MidpointRounding.AwayFromZero);
}
