namespace BankOnTheGo.Domain.DTOs;

public record TransactionDto(
    Guid Id,
    string Type,
    long AmountMinor,
    string Currency,
    DateTime CreatedAtUtc,
    string? Note
);