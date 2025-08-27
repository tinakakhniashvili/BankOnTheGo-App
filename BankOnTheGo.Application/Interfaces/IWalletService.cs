using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Interfaces;

public interface IWalletService
{
    Task<WalletDto> CreateAsync(string userId, WalletRequestDto request, CancellationToken ct);
    Task<IReadOnlyList<WalletDto>> GetMineAsync(string userId, CancellationToken ct);               // changed
    Task<WalletDto> GetAsync(string userId, string currency, CancellationToken ct);                 // new
    Task<TransactionDto> TopUpAsync(string userId, AddTransactionRequestDto request, CancellationToken ct);
    Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(string userId, string? currency, DateTime? from, DateTime? to, CancellationToken ct); // changed
}
