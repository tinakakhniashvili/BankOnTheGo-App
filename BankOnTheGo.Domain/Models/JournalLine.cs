namespace BankOnTheGo.Domain.Models;

public class JournalLine
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JournalEntryId { get; set; }     
    public Guid AccountId { get; set; }          

    public EntryDirection Direction { get; set; } = EntryDirection.Debit;
    public Money Amount { get; set; }          
    
    public static JournalLine Debit(Guid accountId, Money amount)
    {
        if (amount.Amount < 0m) amount = amount.Negate();
        return new JournalLine { AccountId = accountId, Direction = EntryDirection.Debit, Amount = amount };
    }

    public static JournalLine Credit(Guid accountId, Money amount)
    {
        if (amount.Amount < 0m) amount = amount.Negate();
        return new JournalLine { AccountId = accountId, Direction = EntryDirection.Credit, Amount = amount };
    }
}

public enum EntryDirection
{
    Debit = 1,
    Credit = 2
}