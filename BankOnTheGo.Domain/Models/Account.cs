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
    
    public static Account User(Guid userId, string currency, string? name = null)
        => new() { Type = AccountType.User, UserId = userId, Currency = currency.ToUpperInvariant(), Name = name };

    public static Account Fee(string currency)
        => new() { Type = AccountType.Fee, Currency = currency.ToUpperInvariant(), Name = "Fee Account" };

    public static Account Suspense(string currency)
        => new() { Type = AccountType.Suspense, Currency = currency.ToUpperInvariant(), Name = "Suspense Account" };
}

public enum AccountType
{
    User = 1,
    Fee = 2,
    Suspense = 3
}