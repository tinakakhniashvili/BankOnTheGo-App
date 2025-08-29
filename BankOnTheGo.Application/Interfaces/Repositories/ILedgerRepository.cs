using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Interfaces.Repositories;

public interface ILedgerRepository
{
    Task<Account?> GetAccountAsync(Guid accountId, CancellationToken ct = default);
    Task<bool> AccountsExistAsync(IEnumerable<Guid> accountIds, CancellationToken ct = default);
    
    Task<Transaction?> GetTransactionAsync(Guid transactionId, CancellationToken ct = default);
    Task AddTransactionAsync(Transaction tx, CancellationToken ct = default);
    
    Task AddJournalEntryAsync(JournalEntry entry, CancellationToken ct = default);
    
    Task<Money> GetBalanceAsync(Guid accountId, string currency, CancellationToken ct = default);
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}