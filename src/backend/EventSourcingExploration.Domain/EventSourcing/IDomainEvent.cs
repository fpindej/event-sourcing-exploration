namespace EventSourcingExploration.Domain.EventSourcing;

/// <summary>
/// Represents a domain event that occurred in the system.
/// Domain events are immutable records of something that happened.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Gets the unique identifier of the event.
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Gets the timestamp when the event occurred.
    /// </summary>
    DateTime OccurredAt { get; }
    
    /// <summary>
    /// Gets the type name of the event for serialization purposes.
    /// </summary>
    string EventType { get; }
}
