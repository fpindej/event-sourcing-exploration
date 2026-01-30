namespace EventSourcingExploration.Domain.EventSourcing;

/// <summary>
/// Base class for event-sourced aggregates.
/// An aggregate maintains its state by applying domain events.
/// </summary>
public abstract class AggregateRoot
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];
    
    /// <summary>
    /// Gets the unique identifier of the aggregate.
    /// </summary>
    public Guid Id { get; protected set; }
    
    /// <summary>
    /// Gets the current version of the aggregate (number of events applied).
    /// </summary>
    public int Version { get; private set; }
    
    /// <summary>
    /// Gets the uncommitted events that need to be persisted.
    /// </summary>
    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();
    
    /// <summary>
    /// Clears the uncommitted events after they have been persisted.
    /// </summary>
    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }
    
    /// <summary>
    /// Applies an event to the aggregate, updating its state and adding to uncommitted events.
    /// </summary>
    /// <param name="event">The domain event to apply.</param>
    protected void RaiseEvent(IDomainEvent @event)
    {
        ApplyEvent(@event);
        _uncommittedEvents.Add(@event);
    }
    
    /// <summary>
    /// Applies an event to update the aggregate's state without adding to uncommitted events.
    /// Used when replaying events from the event store.
    /// </summary>
    /// <param name="event">The domain event to apply.</param>
    public void ApplyEvent(IDomainEvent @event)
    {
        When(@event);
        Version++;
    }
    
    /// <summary>
    /// Handles the event and updates the aggregate's state.
    /// Must be implemented by derived classes to handle specific event types.
    /// </summary>
    /// <param name="event">The domain event to handle.</param>
    protected abstract void When(IDomainEvent @event);
    
    /// <summary>
    /// Loads the aggregate from a stream of events.
    /// </summary>
    /// <param name="events">The historical events to replay.</param>
    public void LoadFromHistory(IEnumerable<IDomainEvent> events)
    {
        foreach (var @event in events)
        {
            ApplyEvent(@event);
        }
    }
}
