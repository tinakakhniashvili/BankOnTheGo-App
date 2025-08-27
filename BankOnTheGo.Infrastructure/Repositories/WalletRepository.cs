using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Infrastructure.Repositories;

public class WalletRepository : IWalletRepository
{
    private readonly ApplicationDbContext _context;

    public WalletRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<WalletDto?> GetWalletByUserIdAsync(string userId)
    {
        return await _context.Set<WalletDto>()
            .Include(w => w.Transactions)
            .FirstOrDefaultAsync(w => w.UserId == userId);
    }

    public async Task AddWalletAsync(WalletDto walletDto)
    {
        await _context.Set<WalletDto>().AddAsync(walletDto);
    }

    public async Task<List<TransactionDto>> GetTransactionsAsync(string userId)
    {
        return await _context.Set<TransactionDto>()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }

    public async Task AddTransactionAsync(TransactionDto transactionDto)
    {
        await _context.Set<TransactionDto>().AddAsync(transactionDto);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}