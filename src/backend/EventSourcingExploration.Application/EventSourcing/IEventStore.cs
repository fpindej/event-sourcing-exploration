using EventSourcingExploration.Domain.EventSourcing;

namespace EventSourcingExploration.Application.EventSourcing;

/// <summary>
/// Represents a stored event entry for querying and display purposes.
/// </summary>
public record StoredEventOutput(
    Guid EventId,
    Guid AggregateId,
    string AggregateType,
    string EventType,
    string EventData,
    int Version,
    DateTime OccurredAt
);

/// <summary>
/// Interface for the event store that persists and retrieves domain events.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Saves the uncommitted events from an aggregate to the event store.
    /// </summary>
    /// <typeparam name="T">The aggregate type.</typeparam>
    /// <param name="aggregate">The aggregate with uncommitted events.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SaveEventsAsync<T>(T aggregate, CancellationToken cancellationToken = default) 
        where T : AggregateRoot;
    
    /// <summary>
    /// Retrieves all events for an aggregate and reconstructs its state.
    /// </summary>
    /// <typeparam name="T">The aggregate type.</typeparam>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The reconstructed aggregate, or null if not found.</returns>
    Task<T?> GetAggregateAsync<T>(Guid aggregateId, CancellationToken cancellationToken = default) 
        where T : AggregateRoot, new();
    
    /// <summary>
    /// Gets all events for an aggregate in chronological order.
    /// </summary>
    /// <param name="aggregateId">The aggregate identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stored events.</returns>
    Task<List<StoredEventOutput>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all aggregates of a specific type.
    /// </summary>
    /// <param name="aggregateType">The type name of the aggregate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of aggregate IDs.</returns>
    Task<List<Guid>> GetAggregateIdsAsync(string aggregateType, CancellationToken cancellationToken = default);
}
