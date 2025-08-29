using BankOnTheGo.Application.Interfaces.Ledger;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Domain.Models;
using DomainTransaction = BankOnTheGo.Domain.Models.Transaction;

namespace BankOnTheGo.Application.Services.Ledger;

public sealed class LedgerService : ILedgerService
{
    private readonly ILedgerRepository _repo;

    public LedgerService(ILedgerRepository repo) => _repo = repo;

    public async Task<DomainTransaction> CreatePendingTransactionAsync(
        TransactionType type,
        string currency,
        string? reference = null,
        string? metadataJson = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be an ISO 4217 3-letter code.", nameof(currency));

        var tx = new DomainTransaction
        {
            Type = type,
            Currency = currency.ToUpperInvariant(),
            Reference = reference,
            MetadataJson = metadataJson
        };

        await _repo.AddTransactionAsync(tx, ct);
        await _repo.SaveChangesAsync(ct);
        return tx;
    }

    public async Task<Guid> PostAsync(JournalEntry entry, CancellationToken ct = default)
    {
        if (entry is null) throw new ArgumentNullException(nameof(entry));
        if (entry.Lines.Count < 2)
            throw new InvalidOperationException("Journal entry must have at least two lines.");

        var currency = entry.Lines.Select(l => l.Amount.Currency).Distinct().Single();
        if (entry.Lines.Any(l => l.Amount.Amount <= 0m))
            throw new InvalidOperationException("Journal line amounts must be positive.");

        var net = entry.Lines.Sum(l => l.Direction == EntryDirection.Debit ? l.Amount.Amount : -l.Amount.Amount);
        if (net != 0m)
            throw new InvalidOperationException("Journal entry is not balanced (debits != credits).");

        var accountIds = entry.Lines.Select(l => l.AccountId);
        if (!await _repo.AccountsExistAsync(accountIds, ct))
            throw new InvalidOperationException("One or more accounts do not exist.");

        var tx = await _repo.GetTransactionAsync(entry.TransactionId, ct)
                 ?? throw new InvalidOperationException("Transaction not found for journal entry.");

        if (tx.State != TransactionState.Pending)
            throw new InvalidOperationException($"Transaction is not Pending (current: {tx.State}).");

        if (!string.Equals(tx.Currency, currency, StringComparison.Ordinal))
            throw new InvalidOperationException("Journal currency mismatch with transaction currency.");
        
        using var scope = new System.Transactions.TransactionScope(
            System.Transactions.TransactionScopeAsyncFlowOption.Enabled);

        await _repo.AddJournalEntryAsync(entry, ct);
        tx.TransitionTo(TransactionState.Posted);
        await _repo.SaveChangesAsync(ct);

        scope.Complete();
        return entry.Id;
    }

    public Task<Money> GetBalanceAsync(Guid accountId, string currency, CancellationToken ct = default)
        => _repo.GetBalanceAsync(accountId, currency.ToUpperInvariant(), ct);
}
