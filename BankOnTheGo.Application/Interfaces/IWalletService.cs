using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Interfaces;

public interface IWalletService
{
    Task<WalletDto> GetWalletAsync(string userId);
    Task<List<TransactionDto>> GetTransactionHistoryAsync(string userId);
    Task<TransactionDto> AddTransactionAsync(string userId, decimal amount, TransactionType type, string? description = null);
}