using BankOnTheGo.API.Models;

namespace BankOnTheGo.Infrastructure.Repositories;

public interface IWalletRepository
{ 
        WalletModel GetByUserId(int walletId);
        bool WalletExists(int walletId); 
        bool CreateWallet(WalletModel wallet);
}
