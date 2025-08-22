namespace BankOnTheGo.API.Models
{
    public class WalletModel
    {
        public WalletModel(int userId, double currentBalance, int currencyId)
        {
            UserId = userId;
            CurrentBalance = currentBalance;
            CurrencyId = currencyId;
        }
        public int Id { get; set; }
        public int UserId { get; set; }
        public double CurrentBalance { get; set; }
        public int CurrencyId { get; set; }
    }
}
