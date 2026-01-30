using EventSourcingExploration.Application.EventSourcing;
using EventSourcingExploration.Application.Features.EventSourcing;
using EventSourcingExploration.Application.Features.EventSourcing.Dtos;
using EventSourcingExploration.Domain;
using EventSourcingExploration.Domain.EventSourcing.BankAccount;

namespace EventSourcingExploration.Infrastructure.Features.EventSourcing.Services;

/// <summary>
/// Service implementation for bank account operations using event sourcing.
/// </summary>
internal class BankAccountService(IEventStore eventStore) : IBankAccountService
{
    public async Task<Result<BankAccountOutput>> CreateAccountAsync(CreateAccountInput input, CancellationToken ct = default)
    {
        try
        {
            var account = BankAccountAggregate.Create(input.AccountHolder, input.InitialBalance, input.Currency);
            await eventStore.SaveEventsAsync(account, ct);
            return Result<BankAccountOutput>.Success(ToOutput(account));
        }
        catch (ArgumentException ex)
        {
            return Result<BankAccountOutput>.Failure(ex.Message);
        }
    }

    public async Task<Result<BankAccountOutput>> DepositAsync(DepositInput input, CancellationToken ct = default)
    {
        var account = await eventStore.GetAggregateAsync<BankAccountAggregate>(input.AccountId, ct);
        if (account is null)
        {
            return Result<BankAccountOutput>.Failure("Account not found.");
        }

        var result = account.Deposit(input.Amount, input.Description);
        if (!result.IsSuccess)
        {
            return Result<BankAccountOutput>.Failure(result.Error);
        }

        await eventStore.SaveEventsAsync(account, ct);
        return Result<BankAccountOutput>.Success(ToOutput(account));
    }

    public async Task<Result<BankAccountOutput>> WithdrawAsync(WithdrawInput input, CancellationToken ct = default)
    {
        var account = await eventStore.GetAggregateAsync<BankAccountAggregate>(input.AccountId, ct);
        if (account is null)
        {
            return Result<BankAccountOutput>.Failure("Account not found.");
        }

        var result = account.Withdraw(input.Amount, input.Description);
        if (!result.IsSuccess)
        {
            return Result<BankAccountOutput>.Failure(result.Error);
        }

        await eventStore.SaveEventsAsync(account, ct);
        return Result<BankAccountOutput>.Success(ToOutput(account));
    }

    public async Task<Result<BankAccountOutput>> ChangeAccountHolderAsync(ChangeAccountHolderInput input, CancellationToken ct = default)
    {
        var account = await eventStore.GetAggregateAsync<BankAccountAggregate>(input.AccountId, ct);
        if (account is null)
        {
            return Result<BankAccountOutput>.Failure("Account not found.");
        }

        var result = account.ChangeAccountHolder(input.NewName);
        if (!result.IsSuccess)
        {
            return Result<BankAccountOutput>.Failure(result.Error);
        }

        await eventStore.SaveEventsAsync(account, ct);
        return Result<BankAccountOutput>.Success(ToOutput(account));
    }

    public async Task<Result<BankAccountOutput>> CloseAccountAsync(CloseAccountInput input, CancellationToken ct = default)
    {
        var account = await eventStore.GetAggregateAsync<BankAccountAggregate>(input.AccountId, ct);
        if (account is null)
        {
            return Result<BankAccountOutput>.Failure("Account not found.");
        }

        var result = account.Close(input.Reason);
        if (!result.IsSuccess)
        {
            return Result<BankAccountOutput>.Failure(result.Error);
        }

        await eventStore.SaveEventsAsync(account, ct);
        return Result<BankAccountOutput>.Success(ToOutput(account));
    }

    public async Task<Result<BankAccountOutput>> GetAccountAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await eventStore.GetAggregateAsync<BankAccountAggregate>(accountId, ct);
        if (account is null)
        {
            return Result<BankAccountOutput>.Failure("Account not found.");
        }

        return Result<BankAccountOutput>.Success(ToOutput(account));
    }

    public async Task<List<BankAccountOutput>> GetAllAccountsAsync(CancellationToken ct = default)
    {
        var accountIds = await eventStore.GetAggregateIdsAsync(nameof(BankAccountAggregate), ct);
        var accounts = new List<BankAccountOutput>();

        foreach (var id in accountIds)
        {
            var account = await eventStore.GetAggregateAsync<BankAccountAggregate>(id, ct);
            if (account is not null)
            {
                accounts.Add(ToOutput(account));
            }
        }

        return accounts;
    }

    public async Task<Result<TimelineOutput>> GetTimelineAsync(Guid accountId, CancellationToken ct = default)
    {
        var account = await eventStore.GetAggregateAsync<BankAccountAggregate>(accountId, ct);
        if (account is null)
        {
            return Result<TimelineOutput>.Failure("Account not found.");
        }

        var storedEvents = await eventStore.GetEventsAsync(accountId, ct);
        var events = storedEvents.Select(e => new EventOutput(
            e.EventId,
            e.EventType,
            e.EventData,
            e.Version,
            e.OccurredAt
        )).ToList();

        // Build state history by replaying events one by one
        var stateHistory = BuildStateHistory(storedEvents);

        return Result<TimelineOutput>.Success(new TimelineOutput(
            ToOutput(account),
            events,
            stateHistory
        ));
    }

    public async Task<Result<StateAtVersionOutput>> GetStateAtVersionAsync(Guid accountId, int version, CancellationToken ct = default)
    {
        var storedEvents = await eventStore.GetEventsAsync(accountId, ct);
        if (storedEvents.Count == 0)
        {
            return Result<StateAtVersionOutput>.Failure("Account not found.");
        }

        if (version < 1 || version > storedEvents.Count)
        {
            return Result<StateAtVersionOutput>.Failure($"Invalid version. Valid range is 1 to {storedEvents.Count}.");
        }

        var stateHistory = BuildStateHistory(storedEvents);
        var state = stateHistory.FirstOrDefault(s => s.Version == version);

        if (state is null)
        {
            return Result<StateAtVersionOutput>.Failure("Could not reconstruct state at specified version.");
        }

        return Result<StateAtVersionOutput>.Success(state);
    }

    private static List<StateAtVersionOutput> BuildStateHistory(List<StoredEventOutput> storedEvents)
    {
        var stateHistory = new List<StateAtVersionOutput>();
        var tempAggregate = new BankAccountAggregate();

        foreach (var storedEvent in storedEvents)
        {
            var domainEvent = DeserializeEvent(storedEvent);
            tempAggregate.ApplyEvent(domainEvent);

            stateHistory.Add(new StateAtVersionOutput(
                tempAggregate.Version,
                tempAggregate.AccountHolder,
                tempAggregate.Balance,
                tempAggregate.Currency,
                tempAggregate.IsClosed
            ));
        }

        return stateHistory;
    }

    private static BankAccountOutput ToOutput(BankAccountAggregate account)
    {
        return new BankAccountOutput(
            account.Id,
            account.AccountHolder,
            account.Balance,
            account.Currency,
            account.IsClosed,
            account.Version
        );
    }

    private static Domain.EventSourcing.IDomainEvent DeserializeEvent(StoredEventOutput storedEvent)
    {
        return storedEvent.EventType switch
        {
            "AccountCreated" => System.Text.Json.JsonSerializer.Deserialize<AccountCreatedEvent>(storedEvent.EventData, JsonOptions)!,
            "MoneyDeposited" => System.Text.Json.JsonSerializer.Deserialize<MoneyDepositedEvent>(storedEvent.EventData, JsonOptions)!,
            "MoneyWithdrawn" => System.Text.Json.JsonSerializer.Deserialize<MoneyWithdrawnEvent>(storedEvent.EventData, JsonOptions)!,
            "AccountHolderChanged" => System.Text.Json.JsonSerializer.Deserialize<AccountHolderChangedEvent>(storedEvent.EventData, JsonOptions)!,
            "AccountClosed" => System.Text.Json.JsonSerializer.Deserialize<AccountClosedEvent>(storedEvent.EventData, JsonOptions)!,
            _ => throw new InvalidOperationException($"Unknown event type: {storedEvent.EventType}")
        };
    }

    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
}
