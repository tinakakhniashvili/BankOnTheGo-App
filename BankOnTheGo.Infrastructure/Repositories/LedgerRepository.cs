using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Domain.Models;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Infrastructure.Repositories;

public sealed class LedgerRepository : ILedgerRepository
{
    private readonly ApplicationDbContext _db;

    public LedgerRepository(ApplicationDbContext db) => _db = db;
    
    public Task<Account?> GetAccountAsync(Guid accountId, CancellationToken ct = default)
        => _db.Accounts.FirstOrDefaultAsync(a => a.Id == accountId, ct);

    public async Task<bool> AccountsExistAsync(IEnumerable<Guid> accountIds, CancellationToken ct = default)
    {
        var ids = accountIds.Distinct().ToArray();
        if (ids.Length == 0) return true;
        var count = await _db.Accounts.CountAsync(a => ids.Contains(a.Id), ct);
        return count == ids.Length;
    }
    
    public Task<Transaction?> GetTransactionAsync(Guid transactionId, CancellationToken ct = default)
        => _db.Transactions.FirstOrDefaultAsync(t => t.Id == transactionId, ct);

    public Task AddTransactionAsync(Transaction tx, CancellationToken ct = default)
        => _db.Transactions.AddAsync(tx, ct).AsTask();
    
    public Task AddJournalEntryAsync(JournalEntry entry, CancellationToken ct = default)
        => _db.JournalEntries.AddAsync(entry, ct).AsTask();
    
    public async Task<Money> GetBalanceAsync(Guid accountId, string currency, CancellationToken ct = default)
    {
        var amountSum = await _db.JournalLines
            .Where(l => l.AccountId == accountId)
            .Join(_db.JournalEntries,
                  line => line.JournalEntryId,
                  entry => entry.Id,
                  (line, entry) => new { line, entry })
            .Where(x => x.entry.Currency == currency)
            .Select(x => x.line.Direction == EntryDirection.Debit
                         ? x.line.Amount.Amount
                         : -x.line.Amount.Amount)
            .DefaultIfEmpty(0m)
            .SumAsync(ct);

        return new Money(amountSum, currency);
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
