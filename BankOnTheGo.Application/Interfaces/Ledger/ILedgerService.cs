using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Interfaces.Ledger;

public interface ILedgerService
{
    Task<Transaction> CreatePendingTransactionAsync(
        TransactionType type,
        string currency,
        string? reference = null,
        string? metadataJson = null,
        CancellationToken ct = default);
    
    Task<Guid> PostAsync(JournalEntry entry, CancellationToken ct = default);
    
    Task<Money> GetBalanceAsync(Guid accountId, string currency, CancellationToken ct = default);
}