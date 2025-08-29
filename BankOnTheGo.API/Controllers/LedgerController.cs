using BankOnTheGo.Application.Interfaces.Ledger;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/ledger")]
public sealed class LedgerController : ControllerBase
{
    private readonly ILedgerService _ledger;

    public LedgerController(ILedgerService ledger) => _ledger = ledger;

    [HttpPost("transactions")]
    public async Task<IActionResult> CreateTransaction(
        [FromBody] CreateTransactionRequest req,
        CancellationToken ct)
    {
        try
        {
            var tx = await _ledger.CreatePendingTransactionAsync(
                req.Type, req.Currency, req.Reference, req.MetadataJson, ct);

            var dto = new TransactionDto(
                tx.Id,
                tx.Type.ToString(),
                0L,
                tx.Currency,
                tx.CreatedAt.UtcDateTime,
                tx.Reference
            );

            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(Problem(title: "Invalid input", detail: ex.Message));
        }
    }

    [HttpPost("journal-entries")]
    public async Task<IActionResult> PostJournalEntry(
        [FromBody] PostJournalEntryRequest req,
        CancellationToken ct)
    {
        try
        {
            var entry = req.ToDomain();
            var id = await _ledger.PostAsync(entry, ct);
            return Ok(new PostJournalEntryResponse { JournalEntryId = id });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(Problem(title: "Not found", detail: ex.Message));

            if (ex.Message.Contains("not balanced", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("at least two lines", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("positive", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("currency mismatch", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("do not exist", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(Problem(title: "Invalid journal entry", detail: ex.Message));
            }

            return Conflict(Problem(title: "Invalid state", detail: ex.Message));
        }
    }

    [HttpGet("accounts/{accountId:guid}/balance")]
    public async Task<IActionResult> GetAccountBalance(Guid accountId, [FromQuery] string currency, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest(Problem(title: "Invalid input", detail: "Currency is required."));

        var money = await _ledger.GetBalanceAsync(accountId, currency, ct);
        return Ok(MoneyDto.FromDomain(money));
    }

    private static ProblemDetails Problem(string title, string detail) =>
        new() { Title = title, Detail = detail };
}

public sealed record CreateTransactionRequest(
    TransactionType Type,
    string Currency,
    string? Reference,
    string? MetadataJson
);

public sealed class PostJournalEntryRequest
{
    public Guid TransactionId { get; init; }
    public List<JournalLineDto> Lines { get; init; } = new();

    public JournalEntry ToDomain()
    {
        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = TransactionId,
            Lines = Lines.Select(l => new JournalLine
            {
                AccountId = l.AccountId,
                Direction = l.Direction,
                Amount = new Money(l.Amount.Amount, l.Amount.Currency)
            }).ToList()
        };
    }
}

public sealed record JournalLineDto(
    Guid AccountId,
    EntryDirection Direction,
    MoneyDto Amount
);

public sealed record MoneyDto(decimal Amount, string Currency)
{
    public static MoneyDto FromDomain(Money m) => new(m.Amount, m.Currency);
}

public sealed class PostJournalEntryResponse
{
    public Guid JournalEntryId { get; init; }
}
