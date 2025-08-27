using BankOnTheGo.Domain.DTOs;

namespace BankOnTheGo.Domain.Models;

public class AddTransactionRequest
{
    public decimal Amount { get; set; }
    public TransactionType Type { get; set; } 
    public string? Description { get; set; }
}