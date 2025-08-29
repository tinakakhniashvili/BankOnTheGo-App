namespace BankOnTheGo.Domain.Models;

public enum TransactionState
{
    Pending = 1,
    Posted = 2,
    Settled = 3,
    Failed = 4,
    Cancelled = 5
}