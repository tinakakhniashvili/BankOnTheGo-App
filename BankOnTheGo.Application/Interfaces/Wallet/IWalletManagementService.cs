using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Interfaces.Wallet;

public interface IWalletManagementService
{
    Task<WalletDto> CreateAsync(string userId, WalletRequestDto request, CancellationToken ct);
    Task<IReadOnlyList<WalletDto>> GetMineAsync(string userId, CancellationToken ct);
    Task<WalletDto> GetAsync(string userId, string currency, CancellationToken ct);

    Task<IReadOnlyList<TransactionDto>> GetTransactionsAsync(string userId, string? currency, DateTime? from,
        DateTime? to, CancellationToken ct);

    Task<TransactionDto> TopUpAsync(string userId, AddTransactionRequestDto request, CancellationToken ct);
    Task<TransactionDto?> GetTransactionAsync(Guid userId, Guid transactionId, CancellationToken ct);
}