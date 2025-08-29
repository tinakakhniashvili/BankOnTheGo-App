using BankOnTheGo.Domain.Models;

namespace BankOnTheGo.Domain.DTOs.Ledger;

public sealed record JournalLineDto(
    Guid AccountId,
    EntryDirection Direction,
    MoneyDto Amount
);