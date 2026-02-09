namespace DndApp.Inventory.Data.Entities;

public sealed class ProcessedEvent
{
    public Guid EventId { get; set; }

    public DateTimeOffset ProcessedAt { get; set; }
}
