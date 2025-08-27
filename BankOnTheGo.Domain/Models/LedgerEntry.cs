namespace BankOnTheGo.Domain.Models;

public enum LedgerEntryType
{
    TopUp = 1,
    TransferOut = 2,
    TransferIn = 3,
    Withdraw = 4,
    Adjustment = 5
}

public class LedgerEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public Wallet Wallet { get; set; } = default!;

    public long AmountMinor { get; set; }
    public LedgerEntryType Type { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Note { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public string CorrelationId { get; set; } = Guid.NewGuid().ToString("N");
}