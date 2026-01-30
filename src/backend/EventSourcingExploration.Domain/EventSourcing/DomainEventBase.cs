namespace EventSourcingExploration.Domain.EventSourcing;

/// <summary>
/// Base class for all domain events providing common functionality.
/// </summary>
public abstract record DomainEventBase : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public abstract string EventType { get; }
}
