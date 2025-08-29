using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Domain.Models;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _db;

    public WalletRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<Wallet?> GetByUserAndCurrencyAsync(string userId, string currency, CancellationToken ct)
    {
        return _db.Wallets.AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Currency == currency, ct);
    }

    public Task<List<Wallet>> GetAllByUserAsync(string userId, CancellationToken ct)
    {
        return _db.Wallets.AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Currency)
            .ToListAsync(ct);
    }


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
        if (to is not null) q = q.Where(x => x.CreatedAtUtc <= to);
        return q.OrderByDescending(x => x.CreatedAtUtc).ToListAsync(ct);
    }

    public async Task<long> GetBalanceMinorAsync(Guid walletId, CancellationToken ct)
    {
        var sum = await _db.LedgerEntries
            .Where(x => x.WalletId == walletId)
            .SumAsync(x => (long?)x.AmountMinor, ct);
        return sum ?? 0;
    }
}