using BankOnTheGo.API.Mappers;
using BankOnTheGo.Application.Interfaces.Ledger;
using BankOnTheGo.Domain.DTOs.Ledger;
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
        [FromBody] CreateTransactionRequestDto req,
        CancellationToken ct)
    {
        try
        {
            var tx = await _ledger.CreatePendingTransactionAsync(
                req.Type, req.Currency, req.Reference, req.MetadataJson, ct);

            return Ok(LedgerMapping.ToDto(tx));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid input", Detail = ex.Message });
        }
    }

    [HttpPost("journal-entries")]
    public async Task<IActionResult> PostJournalEntry(
        [FromBody] PostJournalEntryRequestDto req,
        CancellationToken ct)
    {
        try
        {
            var entry = LedgerMapping.ToDomain(req);
            var id = await _ledger.PostAsync(entry, ct);
            return Ok(new PostJournalEntryResponseDto { JournalEntryId = id });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                return NotFound(new ProblemDetails { Title = "Not found", Detail = ex.Message });

            if (ex.Message.Contains("not balanced", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("at least two lines", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("positive", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("currency mismatch", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("do not exist", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ProblemDetails { Title = "Invalid journal entry", Detail = ex.Message });
            }

            return Conflict(new ProblemDetails { Title = "Invalid state", Detail = ex.Message });
        }
    }

    [HttpGet("accounts/{accountId:guid}/balance")]
    public async Task<IActionResult> GetAccountBalance(Guid accountId, [FromQuery] string currency, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currency))
            return BadRequest(new ProblemDetails { Title = "Invalid input", Detail = "Currency is required." });

        var money = await _ledger.GetBalanceAsync(accountId, currency, ct);
        return Ok(LedgerMapping.ToDto(money));
    }
}
