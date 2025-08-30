using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Domain.Models;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Infrastructure.Repositories;

public sealed class PaymentLinkRepository : IPaymentLinkRepository
{
    private readonly ApplicationDbContext _db;

    public PaymentLinkRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(PaymentLink link, CancellationToken ct)
    {
        _db.PaymentLinks.Add(link);
        await _db.SaveChangesAsync(ct);
    }

    public Task<PaymentLink?> GetByCodeAsync(string code, CancellationToken ct)
    {
        return _db.PaymentLinks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);
    }

    public async Task UpdateAsync(PaymentLink link, CancellationToken ct)
    {
        _db.PaymentLinks.Update(link);
        await _db.SaveChangesAsync(ct);
    }
}