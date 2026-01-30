namespace MyProject.Domain.EventSourcing.BankAccount;

/// <summary>
/// Event raised when a new bank account is created.
/// </summary>
public sealed record AccountCreatedEvent : DomainEventBase
{
    public override string EventType => "AccountCreated";
    
    public required Guid AccountId { get; init; }
    public required string AccountHolder { get; init; }
    public required decimal InitialBalance { get; init; }
    public required string Currency { get; init; }
}

/// <summary>
/// Event raised when money is deposited into an account.
/// </summary>
public sealed record MoneyDepositedEvent : DomainEventBase
{
    public override string EventType => "MoneyDeposited";
    
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required decimal BalanceAfter { get; init; }
}

/// <summary>
/// Event raised when money is withdrawn from an account.
/// </summary>
public sealed record MoneyWithdrawnEvent : DomainEventBase
{
    public override string EventType => "MoneyWithdrawn";
    
    public required Guid AccountId { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required decimal BalanceAfter { get; init; }
}

/// <summary>
/// Event raised when the account holder name is changed.
/// </summary>
public sealed record AccountHolderChangedEvent : DomainEventBase
{
    public override string EventType => "AccountHolderChanged";
    
    public required Guid AccountId { get; init; }
    public required string OldName { get; init; }
    public required string NewName { get; init; }
}

/// <summary>
/// Event raised when an account is closed.
/// </summary>
public sealed record AccountClosedEvent : DomainEventBase
{
    public override string EventType => "AccountClosed";
    
    public required Guid AccountId { get; init; }
    public required string Reason { get; init; }
    public required decimal FinalBalance { get; init; }
}
