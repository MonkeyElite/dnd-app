namespace DndShop.Contracts;

/// <summary>
/// Wraps event payloads with metadata shared across services.
/// </summary>
/// <typeparam name="T">The event payload type.</typeparam>
public sealed class EventEnvelope<T>
{
    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public Guid EventId { get; init; }

    /// <summary>
    /// Gets the event type name.
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Gets when the event occurred.
    /// </summary>
    public DateTimeOffset OccurredAt { get; init; }

    /// <summary>
    /// Gets the campaign identifier associated with this event.
    /// </summary>
    public Guid CampaignId { get; init; }

    /// <summary>
    /// Gets the correlation identifier used for tracing.
    /// </summary>
    public Guid CorrelationId { get; init; }

    /// <summary>
    /// Gets the event payload.
    /// </summary>
    public T Data { get; init; } = default!;
}
