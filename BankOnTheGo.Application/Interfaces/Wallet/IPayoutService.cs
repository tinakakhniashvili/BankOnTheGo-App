using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Interfaces.Wallet;

public interface IPayoutService
{
    Task<TransactionDto> PayoutAsync(Guid userId, CreatePayoutRequestDto request, CancellationToken ct);
}