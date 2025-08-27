namespace BankOnTheGo.Domain.DTOs;

public class WalletDto
{
    public int Id { get; set; }
    public string UserId { get; set; } 
    public decimal Balance { get; set; } = 0;

    public List<TransactionDto> Transactions { get; set; } = new();
}