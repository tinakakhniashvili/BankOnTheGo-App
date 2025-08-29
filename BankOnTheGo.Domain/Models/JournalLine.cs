namespace BankOnTheGo.Domain.Models;

public class JournalLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JournalEntryId { get; set; }
    public Guid AccountId { get; set; }

    public EntryDirection Direction { get; set; } = EntryDirection.Debit;
    public Money Amount { get; set; }
}

public enum EntryDirection
{
    Debit = 1,
    Credit = 2
}