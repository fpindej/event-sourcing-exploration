using EventSourcingExploration.Application.Features.EventSourcing.Dtos;
using EventSourcingExploration.Domain;

namespace EventSourcingExploration.Application.Features.EventSourcing;

/// <summary>
/// Service interface for bank account operations using event sourcing.
/// </summary>
public interface IBankAccountService
{
    /// <summary>
    /// Creates a new bank account.
    /// </summary>
    Task<Result<BankAccountOutput>> CreateAccountAsync(CreateAccountInput input, CancellationToken ct = default);
    
    /// <summary>
    /// Deposits money into an account.
    /// </summary>
    Task<Result<BankAccountOutput>> DepositAsync(DepositInput input, CancellationToken ct = default);
    
    /// <summary>
    /// Withdraws money from an account.
    /// </summary>
    Task<Result<BankAccountOutput>> WithdrawAsync(WithdrawInput input, CancellationToken ct = default);
    
    /// <summary>
    /// Changes the account holder's name.
    /// </summary>
    Task<Result<BankAccountOutput>> ChangeAccountHolderAsync(ChangeAccountHolderInput input, CancellationToken ct = default);
    
    /// <summary>
    /// Closes an account.
    /// </summary>
    Task<Result<BankAccountOutput>> CloseAccountAsync(CloseAccountInput input, CancellationToken ct = default);
    
    /// <summary>
    /// Gets an account by ID.
    /// </summary>
    Task<Result<BankAccountOutput>> GetAccountAsync(Guid accountId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets all accounts.
    /// </summary>
    Task<List<BankAccountOutput>> GetAllAccountsAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets the timeline for an account, including all events and state history.
    /// </summary>
    Task<Result<TimelineOutput>> GetTimelineAsync(Guid accountId, CancellationToken ct = default);
    
    /// <summary>
    /// Reconstructs the account state at a specific version.
    /// </summary>
    Task<Result<StateAtVersionOutput>> GetStateAtVersionAsync(Guid accountId, int version, CancellationToken ct = default);
}
