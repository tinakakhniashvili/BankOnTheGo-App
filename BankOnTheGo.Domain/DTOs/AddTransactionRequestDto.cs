namespace BankOnTheGo.Domain.DTOs;

public class AddTransactionRequestDto
{
    public long AmountMinor { get; init; }
    public string Currency { get; init; } = default!;
    public string? Note { get; init; }
}