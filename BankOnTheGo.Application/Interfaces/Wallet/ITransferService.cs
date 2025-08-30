using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Interfaces.Wallet;

public interface ITransferService
{
    Task<TransactionDto> TransferAsync(Guid fromUserId, CreateTransferRequestDto request, CancellationToken ct);
}