namespace BankOnTheGo.Domain.Models;

public class Transaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public TransactionType Type { get; set; }
    public TransactionState State { get; private set; } = TransactionState.Pending;

    public string Currency { get; set; } = "USD";
    public string? Reference { get; set; }
    public string? MetadataJson { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? UpdatedAt { get; set; }

    public void TransitionTo(TransactionState next)
    {
        if (!IsValidTransition(State, next))
            throw new InvalidOperationException($"Invalid transaction state transition: {State} → {next}");

        State = next;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static bool IsValidTransition(TransactionState from, TransactionState to)
    {
        return (from, to) switch
        {
            (TransactionState.Pending, TransactionState.Posted) => true,
            (TransactionState.Posted, TransactionState.Settled) => true,
            (TransactionState.Pending, TransactionState.Failed) => true,
            (TransactionState.Posted, TransactionState.Failed) => true,
            (TransactionState.Pending, TransactionState.Cancelled) => true,
            _ => false
        };
    }
}

public enum TransactionType
{
    TopUp = 1,
    Transfer = 2,
    Withdraw = 3,
    Fee = 4
}