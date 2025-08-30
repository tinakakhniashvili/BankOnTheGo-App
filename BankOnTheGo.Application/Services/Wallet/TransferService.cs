using System.Text.Json;
using BankOnTheGo.Application.Interfaces.Ledger;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Application.Interfaces.Wallet;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Services.Wallet;

public sealed class TransferService : ITransferService
{
    private readonly IIdempotencyExecutor _idem;
    private readonly ILedgerService _ledger;
    private readonly IWalletRepository _wallets;

    public TransferService(
        ILedgerService ledger,
        IWalletRepository wallets,
        IIdempotencyExecutor idem)
    {
        _ledger = ledger;
        _wallets = wallets;
        _idem = idem;
    }

    public Task<TransactionDto> TransferAsync(Guid fromUserId, CreateTransferRequestDto req, CancellationToken ct)
    {
        return _idem.ExecuteAsync(fromUserId, req.IdempotencyKey, req, async () =>
        {
            if (req is null) throw new ArgumentNullException(nameof(req));
            if (string.IsNullOrWhiteSpace(req.Currency))
                throw new ArgumentException("Currency is required.", nameof(req));
            if (req.Amount is null)
                throw new ArgumentException("Amount is required.", nameof(req));
            if (!string.Equals(req.Amount.Currency, req.Currency, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Amount currency must match request currency.");
            if (req.Amount.Amount <= 0m)
                throw new InvalidOperationException("Transfer amount must be greater than zero.");
            if (req.ToUserId == Guid.Empty)
                throw new InvalidOperationException("Destination user is required.");
            if (req.ToUserId == fromUserId)
                throw new InvalidOperationException("Cannot transfer to self.");

            var currency = req.Currency;
            
            var fromAccount = await _wallets.GetAccountAsync(fromUserId, currency, ct)
                              ?? throw new InvalidOperationException($"Sender has no {currency} account.");
            var toAccount = await _wallets.GetAccountAsync(req.ToUserId, currency, ct)
                            ?? throw new InvalidOperationException($"Recipient has no {currency} account.");
            
            var balance = await _ledger.GetBalanceAsync(fromAccount.Id, currency, ct);
            if (!string.Equals(balance.Currency, currency, StringComparison.Ordinal))
                throw new InvalidOperationException("Balance currency mismatch.");
            if (balance.Amount < req.Amount.Amount)
                throw new InvalidOperationException("Insufficient funds.");
            
            var amountMinor = ToMinor(req.Amount.Amount, currency);
            var metadata = JsonSerializer.Serialize(new
            {
                kind = "transfer",
                fromUserId,
                toUserId = req.ToUserId,
                amount = req.Amount.Amount,
                amountMinor,
                currency
            });
            
            var pending = await _ledger.CreatePendingTransactionAsync(
                TransactionType.Transfer,
                currency,
                $"transfer:{fromUserId}->{req.ToUserId}",
                metadata,
                ct);
            
            var entry = new JournalEntry
            {
                TransactionId = pending.Id,
                Lines = new List<JournalLine>
                {
                    new()
                    {
                        AccountId = fromAccount.Id,
                        Direction = EntryDirection.Debit,
                        Amount = new Money(req.Amount.Amount, currency)
                    },
                    new()
                    {
                        AccountId = toAccount.Id,
                        Direction = EntryDirection.Credit,
                        Amount = new Money(req.Amount.Amount, currency)
                    }
                }
            };

            var postedTransactionId = await _ledger.PostAsync(entry, ct);
            
            return new TransactionDto(
                postedTransactionId,
                nameof(TransactionType.Transfer),
                amountMinor,
                currency,
                DateTime.UtcNow,
                null);
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
}