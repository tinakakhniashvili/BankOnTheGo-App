using BankOnTheGo.Domain.Wallet;

namespace BankOnTheGo.Domain.DTOs;

public class Wallet
{
    public int Id { get; set; }
    public string UserId { get; set; } 
    public decimal Balance { get; set; } = 0;

    public List<Transaction> Transactions { get; set; } = new();
}