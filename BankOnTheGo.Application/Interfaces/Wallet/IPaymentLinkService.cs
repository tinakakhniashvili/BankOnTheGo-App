using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Application.Interfaces.Wallet;

public interface IPaymentLinkService
{
    Task<PaymentLinkDto> CreateAsync(Guid userId, CreatePaymentLinkRequestDto request, CancellationToken ct);
    Task<PaymentLinkDto?> GetAsync(Guid userId, string code, CancellationToken ct);
    Task<TransactionDto> PayAsync(Guid payerUserId, string code, string? idempotencyKey, CancellationToken ct);
}