using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<Wallet?> GetByUserAndCurrencyAsync(string userId, string currency, CancellationToken ct);
    Task<List<Wallet>> GetAllByUserAsync(string userId, CancellationToken ct);
    Task<Wallet> CreateAsync(string userId, string currency, CancellationToken ct);
    Task AddLedgerAsync(LedgerEntry entry, CancellationToken ct);
    Task<List<LedgerEntry>> GetLedgerAsync(Guid walletId, DateTime? from, DateTime? to, CancellationToken ct);
    Task<long> GetBalanceMinorAsync(Guid walletId, CancellationToken ct);
}