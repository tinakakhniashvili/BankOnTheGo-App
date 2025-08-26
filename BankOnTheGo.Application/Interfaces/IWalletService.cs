using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Wallet;

namespace BankOnTheGo.Application.Interfaces;

public interface IWalletService
{
    Task<Wallet> GetWalletAsync(string userId);
    Task<List<Transaction>> GetTransactionHistoryAsync(string userId);
    Task<Transaction> AddTransactionAsync(string userId, decimal amount, TransactionType type, string? description = null);
}