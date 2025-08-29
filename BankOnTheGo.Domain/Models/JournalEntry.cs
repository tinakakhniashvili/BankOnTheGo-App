namespace BankOnTheGo.Domain.Models;

public class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TransactionId { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    public string Currency { get; private set; } = "USD";

    public List<JournalLine> Lines { get; set; } = new();
}