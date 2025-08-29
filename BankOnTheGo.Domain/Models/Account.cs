namespace BankOnTheGo.Domain.Models;

public class Account
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AccountType Type { get; set; }
    public Guid? UserId { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Name { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}

public enum AccountType
{
    User = 1,
    Fee = 2,
    Suspense = 3
}