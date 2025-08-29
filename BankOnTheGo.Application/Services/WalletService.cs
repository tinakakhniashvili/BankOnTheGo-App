using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Services;

public sealed class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepo;

    public WalletService(IWalletRepository walletRepo)
    {
        _walletRepo = walletRepo;
    }

    public async Task<WalletDto> CreateAsync(string userId, WalletRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(request.Currency));

        var currency = request.Currency.ToUpperInvariant();

        var existing = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency, ct);
        if (existing is not null)
            throw new InvalidOperationException("Wallet already exists for this user and currency.");

        var created = await _walletRepo.CreateAsync(userId, currency, ct);
        return MapWallet(created, 0);
    }

    public async Task<IReadOnlyList<WalletDto>> GetMineAsync(string userId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        var wallets = await _walletRepo.GetAllByUserAsync(userId, ct);
        if (wallets.Count == 0)
            throw new InvalidOperationException("Wallet not found.");

        var result = new List<WalletDto>(wallets.Count);
        foreach (var w in wallets)
        {
            var balance = await _walletRepo.GetBalanceMinorAsync(w.Id, ct);
            result.Add(MapWallet(w, balance));
        }

        return result;
    }

    public async Task<WalletDto> GetAsync(string userId, string currency, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        var wallet = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency.ToUpperInvariant(), ct)
                     ?? throw new InvalidOperationException("Wallet not found.");

        var balance = await _walletRepo.GetBalanceMinorAsync(wallet.Id, ct);
        return MapWallet(wallet, balance);
    }

    public async Task<TransactionDto> TopUpAsync(string userId, AddTransactionRequestDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));
        if (request is null) throw new ArgumentNullException(nameof(request));
        if (request.AmountMinor <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(request.AmountMinor));
        if (string.IsNullOrWhiteSpace(request.Currency) || request.Currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(request.Currency));

        var currency = request.Currency.ToUpperInvariant();

        var wallet = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency, ct)
                     ?? throw new InvalidOperationException("Wallet not found.");

        if (wallet.Status == WalletStatus.Locked)
            throw new InvalidOperationException("Wallet is locked.");

        var entry = new LedgerEntry
        {
            WalletId = wallet.Id,
            AmountMinor = request.AmountMinor,
            Currency = wallet.Currency,
            Type = LedgerEntryType.TopUp,
            Note = request.Note,
            CreatedAtUtc = DateTime.UtcNow
        };

        await _walletRepo.AddLedgerAsync(entry, ct);
        return MapTransaction(entry);
    }

    public async Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(
        string userId, string? currency, DateTime? from, DateTime? to, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User id is required.", nameof(userId));

        Wallet wallet;
        if (!string.IsNullOrWhiteSpace(currency))
        {
            wallet = await _walletRepo.GetByUserAndCurrencyAsync(userId, currency!.ToUpperInvariant(), ct)
                     ?? throw new InvalidOperationException("Wallet not found.");
        }
        else
        {
            var wallets = await _walletRepo.GetAllByUserAsync(userId, ct);
            if (wallets.Count == 0) throw new InvalidOperationException("Wallet not found.");
            if (wallets.Count > 1) throw new ArgumentException("Currency is required when multiple wallets exist.");
            wallet = wallets[0];
        }

        var fromUtc = from?.ToUniversalTime();
        var toUtc = to?.ToUniversalTime();

        var entries = await _walletRepo.GetLedgerAsync(wallet.Id, fromUtc, toUtc, ct);
        return entries.OrderByDescending(e => e.CreatedAtUtc).Select(MapTransaction).ToList();
    }

    // ----------------- Mapping -----------------

    private static WalletDto MapWallet(Wallet w, long balanceMinor)
    {
        return new WalletDto(
            w.Id,
            w.Currency,
            w.Status.ToString(),
            balanceMinor
        );
    }

    private static TransactionDto MapTransaction(LedgerEntry e)
    {
        return new TransactionDto(
            e.Id,
            e.Type.ToString(),
            e.AmountMinor,
            e.Currency,
            e.CreatedAtUtc,
            e.Note
        );
    }
}