namespace MyProject.Domain.EventSourcing.BankAccount;

/// <summary>
/// Represents a bank account aggregate that maintains its state through event sourcing.
/// All state changes are recorded as events.
/// </summary>
public class BankAccountAggregate : AggregateRoot
{
    public string AccountHolder { get; private set; } = string.Empty;
    public decimal Balance { get; private set; }
    public string Currency { get; private set; } = "USD";
    public bool IsClosed { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    
    // Parameterless constructor for reconstruction from events
    public BankAccountAggregate() { }
    
    /// <summary>
    /// Creates a new bank account with the specified details.
    /// </summary>
    public static BankAccountAggregate Create(string accountHolder, decimal initialBalance, string currency = "USD")
    {
        if (string.IsNullOrWhiteSpace(accountHolder))
            throw new ArgumentException("Account holder name is required.", nameof(accountHolder));
        
        if (initialBalance < 0)
            throw new ArgumentException("Initial balance cannot be negative.", nameof(initialBalance));
        
        var account = new BankAccountAggregate();
        var accountId = Guid.NewGuid();
        
        account.RaiseEvent(new AccountCreatedEvent
        {
            AccountId = accountId,
            AccountHolder = accountHolder,
            InitialBalance = initialBalance,
            Currency = currency
        });
        
        return account;
    }
    
    /// <summary>
    /// Deposits money into the account.
    /// </summary>
    public Result Deposit(decimal amount, string description)
    {
        if (IsClosed)
            return Result.Failure("Cannot deposit to a closed account.");
        
        if (amount <= 0)
            return Result.Failure("Deposit amount must be positive.");
        
        var newBalance = Balance + amount;
        
        RaiseEvent(new MoneyDepositedEvent
        {
            AccountId = Id,
            Amount = amount,
            Description = description,
            BalanceAfter = newBalance
        });
        
        return Result.Success();
    }
    
    /// <summary>
    /// Withdraws money from the account.
    /// </summary>
    public Result Withdraw(decimal amount, string description)
    {
        if (IsClosed)
            return Result.Failure("Cannot withdraw from a closed account.");
        
        if (amount <= 0)
            return Result.Failure("Withdrawal amount must be positive.");
        
        if (amount > Balance)
            return Result.Failure("Insufficient funds.");
        
        var newBalance = Balance - amount;
        
        RaiseEvent(new MoneyWithdrawnEvent
        {
            AccountId = Id,
            Amount = amount,
            Description = description,
            BalanceAfter = newBalance
        });
        
        return Result.Success();
    }
    
    /// <summary>
    /// Changes the account holder's name.
    /// </summary>
    public Result ChangeAccountHolder(string newName)
    {
        if (IsClosed)
            return Result.Failure("Cannot modify a closed account.");
        
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure("New account holder name is required.");
        
        if (newName == AccountHolder)
            return Result.Failure("New name must be different from the current name.");
        
        RaiseEvent(new AccountHolderChangedEvent
        {
            AccountId = Id,
            OldName = AccountHolder,
            NewName = newName
        });
        
        return Result.Success();
    }
    
    /// <summary>
    /// Closes the account.
    /// </summary>
    public Result Close(string reason)
    {
        if (IsClosed)
            return Result.Failure("Account is already closed.");
        
        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure("A reason for closing is required.");
        
        RaiseEvent(new AccountClosedEvent
        {
            AccountId = Id,
            Reason = reason,
            FinalBalance = Balance
        });
        
        return Result.Success();
    }
    
    /// <summary>
    /// Applies events to update the aggregate state.
    /// </summary>
    protected override void When(IDomainEvent @event)
    {
        switch (@event)
        {
            case AccountCreatedEvent e:
                Id = e.AccountId;
                AccountHolder = e.AccountHolder;
                Balance = e.InitialBalance;
                Currency = e.Currency;
                break;
                
            case MoneyDepositedEvent e:
                Balance = e.BalanceAfter;
                break;
                
            case MoneyWithdrawnEvent e:
                Balance = e.BalanceAfter;
                break;
                
            case AccountHolderChangedEvent e:
                AccountHolder = e.NewName;
                break;
                
            case AccountClosedEvent:
                IsClosed = true;
                ClosedAt = DateTime.UtcNow;
                break;
        }
    }
}
