using BankOnTheGo.API.Extensions;
using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class WalletController : ControllerBase
{
    private readonly IWalletService _wallets;

    public WalletController(IWalletService wallets)
    {
        _wallets = wallets;
    }

    // ---------- Helpers ----------

    private bool TryGetUser(out Guid userId, out string userIdStr, out IActionResult? error)
    {
        userIdStr = User.GetUserId() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(userIdStr))
        {
            error = Unauthorized(new Response { Status = "Error", Message = "Unauthorized", IsSuccess = false });
            userId = Guid.Empty;
            return false;
        }

        if (!Guid.TryParse(userIdStr, out userId))
        {
            error = Unauthorized(new Response
                { Status = "Error", Message = "Invalid user id format", IsSuccess = false });
            return false;
        }

        error = null;
        return true;
    }

    // ---------- Wallet management ----------

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] WalletRequestDto request, CancellationToken ct)
    {
        if (!TryGetUser(out var userId, out var userIdStr, out var error)) return error!;
        try
        {
            var dto = await _wallets.CreateAsync(userIdStr, request, ct);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists",
                                                       StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        if (!TryGetUser(out _, out var userIdStr, out var error)) return error!;
        try
        {
            var list = await _wallets.GetMineAsync(userIdStr, ct);
            return Ok(list);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
    }

    [HttpGet("{currency}")]
    public async Task<IActionResult> GetByCurrency(string currency, CancellationToken ct)
    {
        if (!TryGetUser(out _, out var userIdStr, out var error)) return error!;
        try
        {
            var dto = await _wallets.GetAsync(userIdStr, currency, ct);
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
    }

    [HttpPost("top-up")]
    public async Task<IActionResult> TopUp([FromBody] AddTransactionRequestDto request, CancellationToken ct)
    {
        if (!TryGetUser(out var userId, out var userIdStr, out var error))
            return
                error!; 
        try
        {
            var dto = await _wallets.TopUpAsync(userIdStr, request, ct);
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("locked", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status423Locked,
                new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] string? currency, [FromQuery] DateTime? from,
        [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (!TryGetUser(out _, out var userIdStr, out var error)) return error!;
        try
        {
            var list = await _wallets.GetTransactionsAsync(userIdStr, currency, from, to, ct);
            return Ok(list);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
    }

    // ---------- Money movement ----------

    [HttpPost("transfers")]
    public async Task<IActionResult> Transfer([FromBody] CreateTransferRequestDto req, CancellationToken ct)
    {
        if (!TryGetUser(out var userId, out _, out var error)) return error!;
        var txn = await _wallets.TransferAsync(userId, req, ct);
        return CreatedAtAction(nameof(GetTransactionById), new { id = txn.Id }, txn);
    }

    [HttpGet("transactions/{id:guid}")]
    public async Task<IActionResult> GetTransactionById(Guid id, CancellationToken ct)
    {
        if (!TryGetUser(out var userId, out _, out var error)) return error!;
        var dto = await _wallets.GetTransactionAsync(userId, id, ct);
        return dto is null ? NotFound() : Ok(dto);
    }

    [HttpPost("payouts")]
    public async Task<IActionResult> Payout([FromBody] CreatePayoutRequestDto req, CancellationToken ct)
    {
        if (!TryGetUser(out var userId, out _, out var error)) return error!;
        var result = await _wallets.PayoutAsync(userId, req, ct);
        return CreatedAtAction(nameof(GetTransactionById), new { id = result.Id }, result);
    }

    // ---------- Payment links ----------

    [HttpPost("payment-links")]
    public async Task<IActionResult> CreatePaymentLink([FromBody] CreatePaymentLinkRequestDto req, CancellationToken ct)
    {
        if (!TryGetUser(out var userId, out _, out var error)) return error!;
        var link = await _wallets.CreatePaymentLinkAsync(userId, req, ct);
        return CreatedAtAction(nameof(GetPaymentLink), new { code = link.Code }, link);
    }

    [HttpGet("payment-links/{code}")]
    public async Task<IActionResult> GetPaymentLink(string code, CancellationToken ct)
    {
        if (!TryGetUser(out var userId, out _, out var error)) return error!;
        var link = await _wallets.GetPaymentLinkAsync(userId, code, ct);
        return link is null ? NotFound() : Ok(link);
    }

    [HttpPost("payment-links/{code}/pay")]
    public async Task<IActionResult> PayPaymentLink(string code,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CancellationToken ct)
    {
        if (!TryGetUser(out var payerUserId, out _, out var error)) return error!;
        var txn = await _wallets.PayPaymentLinkAsync(payerUserId, code, idempotencyKey, ct);
        return CreatedAtAction(nameof(GetTransactionById), new { id = txn.Id }, txn);
    }
}