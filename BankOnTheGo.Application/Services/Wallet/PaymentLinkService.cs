using System.Security.Cryptography;
using System.Text.Json;
using BankOnTheGo.Application.Interfaces.Ledger;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Application.Interfaces.Wallet;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.DTOs.Ledger;
using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Services.Wallet;

public sealed class PaymentLinkService : IPaymentLinkService
{
    private readonly IIdempotencyExecutor _idem;
    private readonly ILedgerService _ledger;
    private readonly IPaymentLinkRepository _links;
    private readonly IWalletRepository _wallets;

    public PaymentLinkService(
        ILedgerService ledger,
        IWalletRepository wallets,
        IPaymentLinkRepository links,
        IIdempotencyExecutor idem)
    {
        _ledger = ledger;
        _wallets = wallets;
        _links = links;
        _idem = idem;
    }

    public async Task<PaymentLinkDto> CreateAsync(Guid userId, CreatePaymentLinkRequestDto req, CancellationToken ct)
    {
        if (req is null) throw new ArgumentNullException(nameof(req));
        if (string.IsNullOrWhiteSpace(req.Currency)) throw new ArgumentException("Currency is required.", nameof(req));
        if (req.ExpiresAt is { } exp && exp <= DateTimeOffset.UtcNow)
            throw new InvalidOperationException("Expiration must be in the future.");

        var ownerAccount = await _wallets.GetAccountAsync(userId, req.Currency, ct);
        if (ownerAccount is null)
            throw new InvalidOperationException($"Owner has no {req.Currency} account.");

        var link = new PaymentLink
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            Code = GenerateCode(),
            Amount = new Money(req.Amount.Amount, req.Amount.Currency),
            Currency = req.Amount.Currency,
            Memo = req.Memo,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = req.ExpiresAt,
            Status = "Active"
        };

        await _links.AddAsync(link, ct);

        return ToDto(link);
    }

    public async Task<PaymentLinkDto?> GetAsync(Guid userId, string code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));

        var link = await _links.GetByCodeAsync(code, ct);
        if (link is null) return null;

        if (link.OwnerUserId != userId) return null;

        return ToDto(link);
    }

    public Task<TransactionDto> PayAsync(Guid payerUserId, string code, string? key, CancellationToken ct)
    {
        return _idem.ExecuteAsync(payerUserId, key ?? "", new { payerUserId, code }, async () =>
        {
            if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.", nameof(code));

            var link = await _links.GetByCodeAsync(code, ct)
                       ?? throw new InvalidOperationException("Payment link not found.");

            if (!string.Equals(link.Status, "Active", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException($"Payment link is not payable (status: {link.Status}).");

            if (link.ExpiresAt is { } exp && exp <= DateTimeOffset.UtcNow)
                throw new InvalidOperationException("Payment link is expired.");

            var currency = link.Currency;
            var amountMinor = ToMinor(link.Amount.Amount, currency);
            var ownerUserId = link.OwnerUserId;

            var payerAccount = await _wallets.GetAccountAsync(payerUserId, currency, ct)
                               ?? throw new InvalidOperationException($"Payer has no {currency} account.");
            var payeeAccount = await _wallets.GetAccountAsync(ownerUserId, currency, ct)
                               ?? throw new InvalidOperationException($"Payee has no {currency} account.");
            
            var metadata = JsonSerializer.Serialize(new
            {
                kind = "payment_link",
                linkId = link.Id,
                code,
                payerUserId,
                payeeUserId = ownerUserId,
                amountMinor,
                currency
            });

            var pending = await _ledger.CreatePendingTransactionAsync(
                TransactionType.Transfer, // adjust if your enum differs
                currency,
                $"payment_link:{code}",
                metadata,
                ct);

            // TODO: Post actual journal entry once you confirm shapes of JournalEntry/JournalLine.
            // var entry = new JournalEntry
            // {
            //     Reference = $"payment_link:{code}",
            //     Lines = new[]
            //     {
            //         JournalLine.Debit (payerAccount.Id,  amountMinor, currency, $"Pay link {code}"),
            //         JournalLine.Credit(payeeAccount.Id, amountMinor, currency, $"Receive link {code}")
            //     }
            // };
            // var postedTransactionId = await _ledger.PostAsync(entry, ct);

            link.Status = "Paid";
            await _links.UpdateAsync(link, ct);

            return new TransactionDto(
                pending.Id,
                nameof(TransactionType.Transfer),
                amountMinor,
                currency,
                DateTime.UtcNow,
                $"Payment for link {code}");
        }, ct);
    }

    // ---------- helpers ----------

    private static string GenerateCode()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToLowerInvariant();
    }
    

    private static PaymentLinkDto ToDto(PaymentLink link)
    {
        var amountDto = new MoneyDto(link.Amount.Amount, link.Currency);

        return new PaymentLinkDto(
            link.Id,
            link.Code,
            amountDto,
            link.Currency,
            link.Memo,
            link.CreatedAt,
            link.ExpiresAt,
            link.Status);
    }

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