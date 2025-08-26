using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.Wallet;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Application.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet?> GetWalletByUserIdAsync(string userId)
    {
        return await _context.Set<Wallet>()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task AddWalletAsync(Wallet wallet)
    {
        await _context.Set<Wallet>().AddAsync(wallet);
    }

    public async Task<List<Transaction>> GetTransactionsAsync(string userId)
    {
        return await _context.Set<Transaction>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        await _context.Set<Transaction>().AddAsync(transaction);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}