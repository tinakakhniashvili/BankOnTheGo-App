using BankOnTheGo.Domain.DTOs.Ledger;

namespace BankOnTheGo.Domain.DTOs;

public sealed record PaymentLinkDto(
    Guid Id,
    string Code,
    MoneyDto Amount,
    string Currency,
    string? Memo,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ExpiresAt,
    string Status);