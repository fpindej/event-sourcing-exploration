namespace MyProject.Application.Features.EventSourcing.Dtos;

// Input DTOs (from API to service)
public record CreateAccountInput(
    string AccountHolder,
    decimal InitialBalance,
    string Currency = "USD"
);

public record DepositInput(
    Guid AccountId,
    decimal Amount,
    string Description
);

public record WithdrawInput(
    Guid AccountId,
    decimal Amount,
    string Description
);

public record ChangeAccountHolderInput(
    Guid AccountId,
    string NewName
);

public record CloseAccountInput(
    Guid AccountId,
    string Reason
);

// Output DTOs (from service to API)
public record BankAccountOutput(
    Guid Id,
    string AccountHolder,
    decimal Balance,
    string Currency,
    bool IsClosed,
    int Version
);

public record EventOutput(
    Guid EventId,
    string EventType,
    string EventData,
    int Version,
    DateTime OccurredAt
);

public record AccountWithEventsOutput(
    BankAccountOutput Account,
    List<EventOutput> Events
);

public record StateAtVersionOutput(
    int Version,
    string AccountHolder,
    decimal Balance,
    string Currency,
    bool IsClosed
);

public record TimelineOutput(
    BankAccountOutput CurrentState,
    List<EventOutput> Events,
    List<StateAtVersionOutput> StateHistory
);
