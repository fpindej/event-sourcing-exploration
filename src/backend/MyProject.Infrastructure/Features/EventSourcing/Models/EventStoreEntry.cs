namespace MyProject.Infrastructure.Features.EventSourcing.Models;

/// <summary>
/// Represents a persisted event in the event store.
/// </summary>
public class EventStoreEntry
{
    public Guid Id { get; set; }
    public Guid AggregateId { get; set; }
    public string AggregateType { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public int Version { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime StoredAt { get; set; }
}
