using BankOnTheGo.Data;
using BankOnTheGo.IRepository;
using BankOnTheGo.Models;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Repository
{
    public class WalletRepository : IWalletRepository
    {
        private readonly DataContext _context;

        public WalletRepository(DataContext context)
        {
            _context = context;
        }
        public WalletModel GetByUserId(int walletId)
        {
            return  _context.Wallets.Where(w => w.Id == walletId).FirstOrDefault();
        }

        public bool WalletExists(int walletId)
        {
            return _context.Wallets.Any(w => w.WallletId == walletId);
        }
    }
}
