namespace BankOnTheGo.Domain.Models;

using System;
using System.Collections.Generic;
using System.Linq;

public class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TransactionId { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    
    public string Currency { get; private set; } = "USD";

    public List<JournalLine> Lines { get; set; } = new();

    public void AddLine(JournalLine line)
    {
        if (Lines.Count == 0)
        {
            Currency = line.Amount.Currency;
        }
        else if (!string.Equals(line.Amount.Currency, Currency, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("All journal lines must use the same currency.");
        }

        Lines.Add(line);
    }

    public void EnsureBalanced()
    {
        if (Lines.Count < 2)
            throw new InvalidOperationException("Journal entry must have at least two lines.");

        var net = Lines.Sum(l => l.Direction == EntryDirection.Debit
            ? l.Amount.Amount
            : -l.Amount.Amount);

        if (net != 0m)
            throw new InvalidOperationException("Journal entry is not balanced (debits != credits).");
    }
}