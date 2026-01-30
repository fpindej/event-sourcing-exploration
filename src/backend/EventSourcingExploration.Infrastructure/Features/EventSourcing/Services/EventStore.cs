using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EventSourcingExploration.Application.EventSourcing;
using EventSourcingExploration.Domain.EventSourcing;
using EventSourcingExploration.Domain.EventSourcing.BankAccount;
using EventSourcingExploration.Infrastructure.Features.EventSourcing.Models;
using EventSourcingExploration.Infrastructure.Persistence;

namespace EventSourcingExploration.Infrastructure.Features.EventSourcing.Services;

/// <summary>
/// Event store implementation using Entity Framework Core and PostgreSQL.
/// </summary>
internal class EventStore(
    EventSourcingExplorationDbContext dbContext,
    ILogger<EventStore> logger) : IEventStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task SaveEventsAsync<T>(T aggregate, CancellationToken cancellationToken = default) 
        where T : AggregateRoot
    {
        var uncommittedEvents = aggregate.UncommittedEvents;
        if (uncommittedEvents.Count == 0)
        {
            return;
        }

        var aggregateType = typeof(T).Name;
        var currentVersion = aggregate.Version - uncommittedEvents.Count;

        foreach (var @event in uncommittedEvents)
        {
            currentVersion++;
            var entry = new EventStoreEntry
            {
                Id = @event.EventId,
                AggregateId = aggregate.Id,
                AggregateType = aggregateType,
                EventType = @event.EventType,
                EventData = JsonSerializer.Serialize(@event, @event.GetType(), JsonOptions),
                Version = currentVersion,
                OccurredAt = @event.OccurredAt,
                StoredAt = DateTime.UtcNow
            };

            dbContext.EventStore.Add(entry);
            logger.LogInformation(
                "Storing event {EventType} for aggregate {AggregateType}/{AggregateId} at version {Version}",
                @event.EventType, aggregateType, aggregate.Id, currentVersion);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        aggregate.ClearUncommittedEvents();
    }

    public async Task<T?> GetAggregateAsync<T>(Guid aggregateId, CancellationToken cancellationToken = default) 
        where T : AggregateRoot, new()
    {
        var entries = await dbContext.EventStore
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        if (entries.Count == 0)
        {
            return null;
        }

        var aggregate = new T();
        var events = entries.Select(DeserializeEvent).ToList();
        aggregate.LoadFromHistory(events);

        return aggregate;
    }

    public async Task<List<StoredEventOutput>> GetEventsAsync(Guid aggregateId, CancellationToken cancellationToken = default)
    {
        var entries = await dbContext.EventStore
            .Where(e => e.AggregateId == aggregateId)
            .OrderBy(e => e.Version)
            .ToListAsync(cancellationToken);

        return entries.Select(e => new StoredEventOutput(
            e.Id,
            e.AggregateId,
            e.AggregateType,
            e.EventType,
            e.EventData,
            e.Version,
            e.OccurredAt
        )).ToList();
    }

    public async Task<List<Guid>> GetAggregateIdsAsync(string aggregateType, CancellationToken cancellationToken = default)
    {
        return await dbContext.EventStore
            .Where(e => e.AggregateType == aggregateType)
            .Select(e => e.AggregateId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static IDomainEvent DeserializeEvent(EventStoreEntry entry)
    {
        return entry.EventType switch
        {
            "AccountCreated" => JsonSerializer.Deserialize<AccountCreatedEvent>(entry.EventData, JsonOptions)!,
            "MoneyDeposited" => JsonSerializer.Deserialize<MoneyDepositedEvent>(entry.EventData, JsonOptions)!,
            "MoneyWithdrawn" => JsonSerializer.Deserialize<MoneyWithdrawnEvent>(entry.EventData, JsonOptions)!,
            "AccountHolderChanged" => JsonSerializer.Deserialize<AccountHolderChangedEvent>(entry.EventData, JsonOptions)!,
            "AccountClosed" => JsonSerializer.Deserialize<AccountClosedEvent>(entry.EventData, JsonOptions)!,
            _ => throw new InvalidOperationException($"Unknown event type: {entry.EventType}")
        };
    }
}
