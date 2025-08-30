using System.Text.Json;
using BankOnTheGo.Application.Interfaces.Ledger;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Application.Interfaces.Wallet;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Services.Wallet;

public sealed class PayoutService : IPayoutService
{
    private readonly IIdempotencyExecutor _idem;
    private readonly ILedgerService _ledger;
    private readonly IWalletRepository _wallets;

    public PayoutService(
        ILedgerService ledger,
        IWalletRepository wallets,
        IIdempotencyExecutor idem)
    {
        _ledger = ledger;
        _wallets = wallets;
        _idem = idem;
    }

    public Task<TransactionDto> PayoutAsync(Guid userId, CreatePayoutRequestDto req, CancellationToken ct)
    {
        return _idem.ExecuteAsync(userId, req.IdempotencyKey, req, async () =>
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (string.IsNullOrWhiteSpace(req.Currency))
                throw new ArgumentException("Currency is required.", nameof(req));
            if (req.Amount.Currency is null ||
                !req.Currency.Equals(req.Amount.Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Amount currency must match request currency.");
            if (req.Amount.Amount <= 0m)
                throw new InvalidOperationException("Payout amount must be greater than zero.");
            if (string.IsNullOrWhiteSpace(req.Destination))
                throw new InvalidOperationException("Payout destination is required.");

            var currency = req.Currency;
            
            var userAccount = await _wallets.GetAccountAsync(userId, currency, ct)
                              ?? throw new InvalidOperationException($"User has no {currency} account.");
            
            var balance = await _ledger.GetBalanceAsync(userAccount.Id, currency, ct);
            if (balance.Currency != currency) throw new InvalidOperationException("Balance currency mismatch.");
            if (balance.Amount < req.Amount.Amount)
                throw new InvalidOperationException("Insufficient funds for payout.");
            
            // TODO: replace with your real repository call / config. See note below.
            var clearingAccountId = await GetPayoutClearingAccountIdAsync(currency, ct);

            // 4) Create a pending transaction (for audit/idempotency)
            var amountMinor = ToMinor(req.Amount.Amount, currency);
            var metadata = JsonSerializer.Serialize(new
            {
                kind = "payout",
                userId,
                destination = req.Destination,
                amount = req.Amount.Amount,
                amountMinor,
                currency
            });

            var pending = await _ledger.CreatePendingTransactionAsync(
                TransactionType.Withdraw,
                currency,
                $"payout:{userId}:{req.Destination}",
                metadata,
                ct);
            
            var entry = new JournalEntry
            {
                TransactionId = pending.Id,
                Lines = new List<JournalLine>
                {
                    new()
                    {
                        AccountId = userAccount.Id,
                        Direction = EntryDirection.Debit, 
                        Amount = new Money(req.Amount.Amount, currency)
                    },
                    new()
                    {
                        AccountId = clearingAccountId,
                        Direction = EntryDirection.Credit, 
                        Amount = new Money(req.Amount.Amount, currency)
                    }
                }
            };

            var postedTransactionId = await _ledger.PostAsync(entry, ct);
            
            return new TransactionDto(
                postedTransactionId,
                nameof(TransactionType.Withdraw),
                amountMinor,
                currency,
                DateTime.UtcNow,
                $"Payout to {req.Destination}");
        }, ct);
    }

    // ---- helpers ----

    private static long ToMinor(decimal amount, string currency)
    {
        var scale = CurrencyDecimals(currency);
        var factor = (decimal)Math.Pow(10, scale);
        return (long)decimal.Round(amount * factor, 0, MidpointRounding.AwayFromZero);
    }

    private static int CurrencyDecimals(string currency)
    {
        return currency.ToUpperInvariant() switch
        {
            "JPY" => 0,
            "KWD" => 3,
            _ => 2
        };
    }

    // TODO: wire this to your ILedgerRepository (or config) once you show me its API.
    private async Task<Guid> GetPayoutClearingAccountIdAsync(string currency, CancellationToken ct)
    {
        throw new InvalidOperationException(
            "Need a clearing/settlement account id. Please show ILedgerRepository (or tell me which account to credit).");
    }
}