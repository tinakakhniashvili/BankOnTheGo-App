using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Domain.Models;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _db;
    public WalletRepository(ApplicationDbContext db) => _db = db;

    public Task<Wallet?> GetByUserAndCurrencyAsync(string userId, string currency, CancellationToken ct) =>
        _db.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Currency == currency, ct);

    public Task<List<Wallet>> GetAllByUserAsync(string userId, CancellationToken ct) =>
        _db.Wallets.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Currency)
            .ToListAsync(ct);

    public Task<Wallet?> GetByUserAsync(string userId, CancellationToken ct) =>
        _db.Wallets.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task<Wallet> CreateAsync(string userId, string currency, CancellationToken ct)
    {
        var w = new Wallet { UserId = userId, Currency = currency };
        _db.Wallets.Add(w);
        await _db.SaveChangesAsync(ct);
        return w;
    }

    public async Task AddLedgerAsync(LedgerEntry entry, CancellationToken ct)
    {
        _db.LedgerEntries.Add(entry);
        await _db.SaveChangesAsync(ct);
    }

    public Task<List<LedgerEntry>> GetLedgerAsync(Guid walletId, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var q = _db.LedgerEntries.AsNoTracking().Where(x => x.WalletId == walletId);
        if (from is not null) q = q.Where(x => x.CreatedAtUtc >= from);
        if (to   is not null) q = q.Where(x => x.CreatedAtUtc <= to);
        return q.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
    }

    public async Task<long> GetBalanceMinorAsync(Guid walletId, CancellationToken ct)
    {
        var sum = await _db.LedgerEntries
            .Where(x => x.WalletId == walletId)
            .SumAsync(x => (long?)x.AmountMinor, ct);
        return sum ?? 0;
    }

    public async Task UpdateAsync(Wallet wallet, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(wallet);

        var existing = await _db.Wallets
            .AsTracking()
            .FirstOrDefaultAsync(w => w.Id == wallet.Id, ct);

        if (existing is null)
            throw new KeyNotFoundException($"Wallet '{wallet.Id}' not found.");
        
        if (!string.Equals(existing.UserId, wallet.UserId, StringComparison.Ordinal))
            throw new InvalidOperationException("UserId cannot be changed.");

        if (!string.Equals(existing.Currency, wallet.Currency, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Currency cannot be changed.");
        
        existing.Status = wallet.Status;

        await _db.SaveChangesAsync(ct);
    }
}