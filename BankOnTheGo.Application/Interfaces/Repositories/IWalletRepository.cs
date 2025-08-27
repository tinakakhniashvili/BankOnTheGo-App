using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<WalletDto?> GetWalletByUserIdAsync(string userId);
    Task AddWalletAsync(WalletDto walletDto);
    Task<List<TransactionDto>> GetTransactionsAsync(string userId);
    Task AddTransactionAsync(TransactionDto transactionDto);
    Task SaveChangesAsync();
}