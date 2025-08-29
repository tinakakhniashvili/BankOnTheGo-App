namespace BankOnTheGo.Domain.DTOs.Ledger;

public sealed class PostJournalEntryRequestDto
{
    public Guid TransactionId { get; init; }
    public List<JournalLineDto> Lines { get; init; } = new();
}