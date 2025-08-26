using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Wallet;

namespace BankOnTheGo.Application.Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepository;

        public WalletService(IWalletRepository walletRepository)
        {
            _walletRepository = walletRepository;
        }

        public async Task<Wallet> GetWalletAsync(string userId)
        {
            var wallet = await _walletRepository.GetWalletByUserIdAsync(userId);

            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 0 };
                await _walletRepository.AddWalletAsync(wallet);
                await _walletRepository.SaveChangesAsync();
            }

            return wallet;
        }

        public async Task<List<Transaction>> GetTransactionHistoryAsync(string userId)
        {
            return await _walletRepository.GetTransactionsAsync(userId);
        }

        public async Task<Transaction> AddTransactionAsync(string userId, decimal amount, TransactionType type, string? description = null)
        {
            var wallet = await GetWalletAsync(userId);

            if (type == TransactionType.Withdrawal && wallet.Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            wallet.Balance += (type == TransactionType.Deposit || type == TransactionType.Transfer ? amount : -amount);

            var transaction = new Transaction
            {
                UserId = userId,
                Amount = amount,
                Type = type,
                Description = description,
                Date = DateTime.UtcNow
            };

            await _walletRepository.AddTransactionAsync(transaction);
            await _walletRepository.SaveChangesAsync();

            return transaction;
        }
    }
}