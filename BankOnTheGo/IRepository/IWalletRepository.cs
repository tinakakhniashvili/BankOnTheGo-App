using BankOnTheGo.Models;

namespace BankOnTheGo.IRepository
{
    public interface IWalletRepository
    {
        WalletModel GetByUserId(int walletId);
        bool WalletExists(int walletId);
        bool CreateWallet(WalletModel wallet);
    }
}
