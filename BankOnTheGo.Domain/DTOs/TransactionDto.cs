namespace BankOnTheGo.Domain.DTOs;

public enum TransactionType
{
    Deposit,
    Withdrawal,
    Transfer
}

public class TransactionDto
{
    public int Id { get; set; }
    public string UserId { get; set; } 
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public string? Description { get; set; }
}