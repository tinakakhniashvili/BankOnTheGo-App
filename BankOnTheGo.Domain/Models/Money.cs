using System.Globalization;

namespace BankOnTheGo.Domain.Models;
public readonly record struct Money
{
    public decimal Amount { get; }
    public string Currency { get; } 

    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        Currency = currency.ToUpperInvariant();
        Amount = amount;
    }

    public static Money Zero(string currency) => new(0m, currency);

    public bool IsZero => Amount == 0m;

    public Money Negate() => new(-Amount, Currency);

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b) => a + b.Negate();

    public static Money operator *(Money a, decimal factor) => new(a.Amount * factor, a.Currency);

    public static Money Min(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount <= b.Amount ? a : b;
    }

    public static Money Max(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return a.Amount >= b.Amount ? a : b;
    }

    public override string ToString() =>
        string.Create(CultureInfo.InvariantCulture, $"{Amount:0.##} {Currency}");

    public static void EnsureSameCurrency(Money a, Money b)
    {
        if (!string.Equals(a.Currency, b.Currency, StringComparison.Ordinal))
            throw new InvalidOperationException("Currency mismatch.");
    }
}