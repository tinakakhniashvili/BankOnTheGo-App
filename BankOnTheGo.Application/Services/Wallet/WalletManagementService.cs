using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Application.Interfaces.Wallet;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Models;
using DomainWallet = BankOnTheGo.Domain.Models.Wallet;

namespace BankOnTheGo.Application.Services.Wallet;

public sealed class WalletManagementService : IWalletManagementService
{
    private readonly IWalletRepository _wallets;

    public WalletManagementService(IWalletRepository wallets)
    {
        _wallets = wallets;
    }

    public async Task<WalletDto> CreateAsync(string userId, WalletRequestDto request, CancellationToken ct)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Currency is required.", nameof(request));

        var existing = await _wallets.GetByUserAndCurrencyAsync(userId, request.Currency, ct);
        if (existing is not null)
        {
            var bal = await _wallets.GetBalanceMinorAsync(existing.Id, ct);
            return new WalletDto(existing.Id, existing.Currency, existing.Status.ToString(), bal);
        }

        var created = await _wallets.CreateAsync(userId, request.Currency, ct);
        var balance = await _wallets.GetBalanceMinorAsync(created.Id, ct);

        return new WalletDto(created.Id, created.Currency, created.Status.ToString(), balance);
    }

    public async Task<IReadOnlyList<WalletDto>> GetMineAsync(string userId, CancellationToken ct)
    {
        var list = await _wallets.GetAllByUserAsync(userId, ct);
        var result = new List<WalletDto>(list.Count);

        foreach (var w in list)
        {
            var bal = await _wallets.GetBalanceMinorAsync(w.Id, ct);
            result.Add(new WalletDto(w.Id, w.Currency, w.Status.ToString(), bal));
        }

        return result;
    }

    public async Task<WalletDto> GetAsync(string userId, string currency, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        var wallet = await _wallets.GetByUserAndCurrencyAsync(userId, currency, ct)
                     ?? throw new InvalidOperationException($"Wallet not found for currency {currency}.");

        var bal = await _wallets.GetBalanceMinorAsync(wallet.Id, ct);
        return new WalletDto(wallet.Id, wallet.Currency, wallet.Status.ToString(), bal);
    }

    public async Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(
        string userId, string? currency, DateTime? from, DateTime? to, CancellationToken ct)
    {
        List<DomainWallet> wallets;
        if (currency is null)
        {
            wallets = await _wallets.GetAllByUserAsync(userId, ct);
        }
        else
        {
            var single = await _wallets.GetByUserAndCurrencyAsync(userId, currency, ct)
                         ?? throw new InvalidOperationException($"Wallet not found for currency {currency}.");
            wallets = new List<DomainWallet> { single };
        }

        var txs = new List<TransactionDto>();

        foreach (var w in wallets)
        {
            var entries = await _wallets.GetLedgerAsync(w.Id, from, to, ct);
            foreach (var e in entries)
                txs.Add(new TransactionDto(
                    e.Id,
                    e.Type.ToString(),
                    e.AmountMinor,
                    e.Currency,
                    e.CreatedAtUtc,
                    e.Note
                ));
        }

        txs.Sort((a, b) => b.CreatedAtUtc.CompareTo(a.CreatedAtUtc));
        return txs;
    }

    public async Task<TransactionDto> TopUpAsync(string userId, AddTransactionRequestDto request, CancellationToken ct)
    {
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Currency))
            throw new ArgumentException("Currency is required.", nameof(request));
        if (request.AmountMinor <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(request));

        var wallet = await _wallets.GetByUserAndCurrencyAsync(userId, request.Currency, ct)
                     ?? throw new InvalidOperationException($"Wallet not found for currency {request.Currency}.");

        var entry = new LedgerEntry
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            AmountMinor = request.AmountMinor,
            Currency = request.Currency,
            Type = LedgerEntryType.TopUp,
            Note = request.Note,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _wallets.AddLedgerAsync(entry, ct);

        return new TransactionDto(
            entry.Id,
            entry.Type.ToString(),
            entry.AmountMinor,
            entry.Currency,
            entry.CreatedAtUtc,
            entry.Note
        );
    }

    public async Task<TransactionDto?> GetTransactionAsync(Guid userId, Guid transactionId, CancellationToken ct)
    {
        var wallets = await _wallets.GetAllByUserAsync(userId.ToString(), ct);
        foreach (var w in wallets)
        {
            var entries = await _wallets.GetLedgerAsync(w.Id, null, null, ct);
            var e = entries.FirstOrDefault(x => x.Id == transactionId);
            if (e is not null)
                return new TransactionDto(
                    e.Id,
                    e.Type.ToString(),
                    e.AmountMinor,
                    e.Currency,
                    e.CreatedAtUtc,
                    e.Note
                );
        }

        return null;
    }
}