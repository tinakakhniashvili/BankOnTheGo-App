using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Application.Interfaces.Repositories;

public interface IPaymentLinkRepository
{
    Task AddAsync(PaymentLink link, CancellationToken ct);
    Task<PaymentLink?> GetByCodeAsync(string code, CancellationToken ct);
    Task UpdateAsync(PaymentLink link, CancellationToken ct);
}