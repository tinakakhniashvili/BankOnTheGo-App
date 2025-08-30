using BankOnTheGo.Domain.DTOs.Ledger;

namespace BankOnTheGo.Domain.DTOs;

public sealed record CreatePayoutRequestDto(
    MoneyDto Amount,
    string Currency,
    string Destination, // IBAN / card / provider account
    string? IdempotencyKey);