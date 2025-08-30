using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Interfaces.Repositories;

public interface IWalletRepository
{
    Task<Domain.Models.Wallet?> GetByUserAndCurrencyAsync(string userId, string currency, CancellationToken ct);
    Task<List<Domain.Models.Wallet>> GetAllByUserAsync(string userId, CancellationToken ct);
    Task<Domain.Models.Wallet> CreateAsync(string userId, string currencyw, CancellationToken ct);
    Task AddLedgerAsync(LedgerEntry entry, CancellationToken ct);
    Task<List<LedgerEntry>> GetLedgerAsync(Guid walletId, DateTime? from, DateTime? to, CancellationToken ct);
    Task<long> GetBalanceMinorAsync(Guid walletId, CancellationToken ct);
    Task<Account?> GetAccountAsync(Guid userId, string currency, CancellationToken ct);
}