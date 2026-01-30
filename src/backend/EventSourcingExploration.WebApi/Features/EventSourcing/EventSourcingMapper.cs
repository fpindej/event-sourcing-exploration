using System.Text.Json;
using EventSourcingExploration.Application.Features.EventSourcing.Dtos;

namespace EventSourcingExploration.WebApi.Features.EventSourcing;

public static class EventSourcingMapper
{
    public static CreateAccountInput ToInput(this Dtos.CreateAccountRequest request)
        => new(request.AccountHolder, request.InitialBalance, request.Currency);

    public static DepositInput ToInput(this Dtos.DepositRequest request, Guid accountId)
        => new(accountId, request.Amount, request.Description);

    public static WithdrawInput ToInput(this Dtos.WithdrawRequest request, Guid accountId)
        => new(accountId, request.Amount, request.Description);

    public static ChangeAccountHolderInput ToInput(this Dtos.ChangeAccountHolderRequest request, Guid accountId)
        => new(accountId, request.NewName);

    public static CloseAccountInput ToInput(this Dtos.CloseAccountRequest request, Guid accountId)
        => new(accountId, request.Reason);

    public static Dtos.BankAccountResponse ToResponse(this BankAccountOutput output)
        => new(output.Id, output.AccountHolder, output.Balance, output.Currency, output.IsClosed, output.Version);

    public static Dtos.EventResponse ToResponse(this EventOutput output)
        => new(
            output.EventId,
            output.EventType,
            JsonSerializer.Deserialize<object>(output.EventData) ?? new object(),
            output.Version,
            output.OccurredAt
        );

    public static Dtos.StateSnapshotResponse ToResponse(this StateAtVersionOutput output)
        => new(output.Version, output.AccountHolder, output.Balance, output.Currency, output.IsClosed);

    public static Dtos.TimelineResponse ToResponse(this TimelineOutput output)
        => new(
            output.CurrentState.ToResponse(),
            output.Events.Select(e => e.ToResponse()).ToList(),
            output.StateHistory.Select(s => s.ToResponse()).ToList()
        );
}
