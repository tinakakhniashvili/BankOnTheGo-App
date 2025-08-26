using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Wallet;

namespace BankOnTheGo.Application.Interfaces;

public interface IWalletRepository
{
    Task<Wallet?> GetWalletByUserIdAsync(string userId);
    Task AddWalletAsync(Wallet wallet);
    Task<List<Transaction>> GetTransactionsAsync(string userId);
    Task AddTransactionAsync(Transaction transaction);
    Task SaveChangesAsync();
}