using BankOnTheGo.Domain.DTOs;
using BankOnTheGo.Domain.DTOs.Ledger;
using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.API.Mappers;

public static class LedgerMapping
{
    public static MoneyDto ToDto(Money m)
    {
        return new MoneyDto(m.Amount, m.Currency);
    }

    public static TransactionDto ToDto(Transaction tx)
    {
        return new TransactionDto(
            tx.Id,
            tx.Type.ToString(),
            0L,
            tx.Currency,
            tx.CreatedAt.UtcDateTime,
            tx.Reference
        );
    }

    public static JournalEntry ToDomain(PostJournalEntryRequestDto dto)
    {
        return new JournalEntry
        {
            Id = Guid.NewGuid(),
            TransactionId = dto.TransactionId,
            Lines = dto.Lines.Select(l => new JournalLine
            {
                AccountId = l.AccountId,
                Direction = l.Direction,
                Amount = new Money(l.Amount.Amount, l.Amount.Currency)
            }).ToList()
        };
    }
}