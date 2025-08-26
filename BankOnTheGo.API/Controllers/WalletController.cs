using System.Security.Claims;
using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankOnTheGo.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;

    public WalletController(IWalletService walletService)
    {
        _walletService = walletService;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetWallet()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var wallet = await _walletService.GetWalletAsync(userId);
        return Ok(wallet);
    }
    
    [HttpGet("transactions")]
    public async Task<IActionResult> GetTransactionHistory()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        var transactions = await _walletService.GetTransactionHistoryAsync(userId);
        return Ok(transactions);
    }
    
    [HttpPost("transaction")]
    public async Task<IActionResult> AddTransaction([FromBody] AddTransactionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
            return Unauthorized();

        try
        {
            var transaction = await _walletService.AddTransactionAsync(
                userId,
                request.Amount,
                request.Type,
                request.Description
            );

            return Ok(transaction);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}