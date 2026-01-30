using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EventSourcingExploration.Application.Features.EventSourcing;
using EventSourcingExploration.WebApi.Features.EventSourcing.Dtos;

namespace EventSourcingExploration.WebApi.Features.EventSourcing;

/// <summary>
/// Controller for exploring event sourcing concepts with a bank account example.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Allow anonymous for exploration purposes
public class EventSourcingController(IBankAccountService bankAccountService) : ControllerBase
{
    /// <summary>
    /// Creates a new bank account.
    /// </summary>
    [HttpPost("accounts")]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BankAccountResponse>> CreateAccount(
        [FromBody] CreateAccountRequest request,
        CancellationToken ct)
    {
        var result = await bankAccountService.CreateAccountAsync(request.ToInput(), ct);
        
        if (!result.IsSuccess)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetAccount),
            new { accountId = result.Value!.Id },
            result.Value.ToResponse());
    }

    /// <summary>
    /// Gets all bank accounts.
    /// </summary>
    [HttpGet("accounts")]
    [ProducesResponseType(typeof(List<BankAccountResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BankAccountResponse>>> GetAllAccounts(CancellationToken ct)
    {
        var accounts = await bankAccountService.GetAllAccountsAsync(ct);
        return Ok(accounts.Select(a => a.ToResponse()).ToList());
    }

    /// <summary>
    /// Gets a bank account by ID.
    /// </summary>
    [HttpGet("accounts/{accountId:guid}")]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BankAccountResponse>> GetAccount(Guid accountId, CancellationToken ct)
    {
        var result = await bankAccountService.GetAccountAsync(accountId, ct);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value!.ToResponse());
    }

    /// <summary>
    /// Deposits money into an account.
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/deposit")]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BankAccountResponse>> Deposit(
        Guid accountId,
        [FromBody] DepositRequest request,
        CancellationToken ct)
    {
        var result = await bankAccountService.DepositAsync(request.ToInput(accountId), ct);
        
        if (!result.IsSuccess)
        {
            if (result.Error == "Account not found.")
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value!.ToResponse());
    }

    /// <summary>
    /// Withdraws money from an account.
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/withdraw")]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BankAccountResponse>> Withdraw(
        Guid accountId,
        [FromBody] WithdrawRequest request,
        CancellationToken ct)
    {
        var result = await bankAccountService.WithdrawAsync(request.ToInput(accountId), ct);
        
        if (!result.IsSuccess)
        {
            if (result.Error == "Account not found.")
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value!.ToResponse());
    }

    /// <summary>
    /// Changes the account holder's name.
    /// </summary>
    [HttpPatch("accounts/{accountId:guid}/holder")]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BankAccountResponse>> ChangeAccountHolder(
        Guid accountId,
        [FromBody] ChangeAccountHolderRequest request,
        CancellationToken ct)
    {
        var result = await bankAccountService.ChangeAccountHolderAsync(request.ToInput(accountId), ct);
        
        if (!result.IsSuccess)
        {
            if (result.Error == "Account not found.")
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value!.ToResponse());
    }

    /// <summary>
    /// Closes an account.
    /// </summary>
    [HttpPost("accounts/{accountId:guid}/close")]
    [ProducesResponseType(typeof(BankAccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BankAccountResponse>> CloseAccount(
        Guid accountId,
        [FromBody] CloseAccountRequest request,
        CancellationToken ct)
    {
        var result = await bankAccountService.CloseAccountAsync(request.ToInput(accountId), ct);
        
        if (!result.IsSuccess)
        {
            if (result.Error == "Account not found.")
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value!.ToResponse());
    }

    /// <summary>
    /// Gets the complete timeline for an account, including all events and state history.
    /// This is the key endpoint for visualizing event sourcing.
    /// </summary>
    [HttpGet("accounts/{accountId:guid}/timeline")]
    [ProducesResponseType(typeof(TimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TimelineResponse>> GetTimeline(Guid accountId, CancellationToken ct)
    {
        var result = await bankAccountService.GetTimelineAsync(accountId, ct);
        
        if (!result.IsSuccess)
        {
            return NotFound(result.Error);
        }

        return Ok(result.Value!.ToResponse());
    }

    /// <summary>
    /// Reconstructs the account state at a specific version (point in time).
    /// Demonstrates the ability to "time travel" with event sourcing.
    /// </summary>
    [HttpGet("accounts/{accountId:guid}/state/{version:int}")]
    [ProducesResponseType(typeof(StateSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StateSnapshotResponse>> GetStateAtVersion(
        Guid accountId,
        int version,
        CancellationToken ct)
    {
        var result = await bankAccountService.GetStateAtVersionAsync(accountId, version, ct);
        
        if (!result.IsSuccess)
        {
            if (result.Error == "Account not found.")
            {
                return NotFound(result.Error);
            }
            return BadRequest(result.Error);
        }

        return Ok(result.Value!.ToResponse());
    }
}
