using BankOnTheGo.Domain.DTOs.Ledger;

namespace BankOnTheGo.Domain.DTOs;

public sealed record CreateTransferRequestDto(
    Guid ToUserId,
    MoneyDto Amount,
    string Currency,
    string? Memo,
    string? IdempotencyKey);