namespace BankOnTheGo.Domain.Models;

public class PaymentLink
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public string Code { get; set; } = default!;
    public string Currency { get; set; } = default!;
    public Money Amount { get; set; } = new(0m, "USD");
    public string? Memo { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
    public string Status { get; set; } = "Active";
}