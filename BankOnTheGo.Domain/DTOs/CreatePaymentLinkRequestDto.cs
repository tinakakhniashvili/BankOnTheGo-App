using BankOnTheGo.Domain.DTOs.Ledger;

namespace BankOnTheGo.Domain.DTOs;

public sealed record CreatePaymentLinkRequestDto(
    MoneyDto Amount,
    string Currency,
    string? Memo,
    DateTimeOffset? ExpiresAt);