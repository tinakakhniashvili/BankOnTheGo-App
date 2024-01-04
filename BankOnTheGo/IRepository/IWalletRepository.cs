using BankOnTheGo.Models;

namespace BankOnTheGo.IRepository
{
    public interface IWalletRepository
    {
        public WalletModel GetByUserId(int walletId);
        public bool WalletExists(int walletId);

        public bool CreateWallet(WalletModel wallet);
    }
}
