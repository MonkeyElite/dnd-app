namespace DndApp.Sales.Data.Entities;

public sealed class OutboxMessage
{
    public Guid OutboxMessageId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string Type { get; set; } = string.Empty;

    public Guid AggregateId { get; set; }

    public Guid CampaignId { get; set; }

    public Guid CorrelationId { get; set; }

    public string PayloadJson { get; set; } = string.Empty;

    public DateTimeOffset? PublishedAt { get; set; }

    public int PublishAttempts { get; set; }

    public string? LastError { get; set; }
}
