namespace BankOnTheGo.Models
{
    public class WalletModel
    {
        public WalletModel(int id, int walletId, double currentBalance, int currencyId)
        {
            Id = id;
            WallletId = walletId;
            CurrentBalance = currentBalance;
            CurrencyId = currencyId;
        }
        public int Id { get; set; }
        public int WallletId { get; set; }
        public double CurrentBalance { get; set; }
        public int CurrencyId { get; set; }
    }
}
