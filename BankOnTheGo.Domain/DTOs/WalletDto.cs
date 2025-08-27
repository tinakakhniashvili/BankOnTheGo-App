namespace BankOnTheGo.Domain.DTOs;

public record WalletDto(Guid Id, string Currency, string Status, long BalanceMinor);