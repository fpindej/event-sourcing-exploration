namespace MyProject.WebApi.Features.EventSourcing.Dtos;

// Request DTOs
public record CreateAccountRequest(
    string AccountHolder,
    decimal InitialBalance,
    string Currency = "USD"
);

public record DepositRequest(
    decimal Amount,
    string Description
);

public record WithdrawRequest(
    decimal Amount,
    string Description
);

public record ChangeAccountHolderRequest(
    string NewName
);

public record CloseAccountRequest(
    string Reason
);

// Response DTOs
public record BankAccountResponse(
    Guid Id,
    string AccountHolder,
    decimal Balance,
    string Currency,
    bool IsClosed,
    int Version
);

public record EventResponse(
    Guid EventId,
    string EventType,
    object EventData,
    int Version,
    DateTime OccurredAt
);

public record TimelineResponse(
    BankAccountResponse Account,
    List<EventResponse> Events,
    List<StateSnapshotResponse> StateHistory
);

public record StateSnapshotResponse(
    int Version,
    string AccountHolder,
    decimal Balance,
    string Currency,
    bool IsClosed
);
