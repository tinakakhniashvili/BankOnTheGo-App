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
    private readonly IWalletService _walletService;
    public WalletController(IWalletService walletService) => _walletService = walletService;

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] WalletRequestDto request, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized(new Response { Status = "Error", Message = "Unauthorized", IsSuccess = false });

        try
        {
            var dto = await _walletService.CreateAsync(userId, request, ct);
            return Ok(dto);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized(new Response { Status = "Error", Message = "Unauthorized", IsSuccess = false });

        try
        {
            var list = await _walletService.GetMineAsync(userId, ct);
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
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized(new Response { Status = "Error", Message = "Unauthorized", IsSuccess = false });

        try
        {
            var dto = await _walletService.GetAsync(userId, currency, ct);
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
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized(new Response { Status = "Error", Message = "Unauthorized", IsSuccess = false });

        try
        {
            var dto = await _walletService.TopUpAsync(userId, request, ct);
            return Ok(dto);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return NotFound(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("locked", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(StatusCodes.Status423Locked, new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new Response { Status = "Error", Message = ex.Message, IsSuccess = false });
        }
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactions([FromQuery] string? currency, [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
            return Unauthorized(new Response { Status = "Error", Message = "Unauthorized", IsSuccess = false });

        try
        {
            var list = await _walletService.GetTransactionsAsync(userId, currency, from, to, ct);
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
}
