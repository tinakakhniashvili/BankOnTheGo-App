using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Domain.DTOs.Ledger;

public sealed record CreateTransactionRequestDto(
    TransactionType Type,
    string Currency,
    string? Reference,
    string? MetadataJson
);