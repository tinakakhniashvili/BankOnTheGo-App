using BankOnTheGo.Domain.Authentication.User;

namespace BankOnTheGo.Domain.Models;

public enum WalletStatus { Active = 1, Locked = 2 }

public class Wallet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public string Currency { get; set; } = "USD";
    public WalletStatus Status { get; set; } = WalletStatus.Active;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<LedgerEntry> Ledger { get; set; } = new List<LedgerEntry>();
}