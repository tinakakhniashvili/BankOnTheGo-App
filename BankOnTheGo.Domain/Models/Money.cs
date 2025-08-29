using System.Globalization;

namespace BankOnTheGo.Domain.Models;

public readonly record struct Money
{
    public Money(decimal amount, string currency)
    {
        if (string.IsNullOrWhiteSpace(currency) || currency.Length != 3)
            throw new ArgumentException("Currency must be a 3-letter ISO code.", nameof(currency));

        Currency = currency.ToUpperInvariant();
        Amount = amount;
    }

    public decimal Amount { get; }
    public string Currency { get; }

    public bool IsZero => Amount == 0m;

    public static Money Zero(string currency)
    {
        return new Money(0m, currency);
    }

    public Money Negate()
    {
        return new Money(-Amount, Currency);
    }

    public static Money operator +(Money a, Money b)
    {
        EnsureSameCurrency(a, b);
        return new Money(a.Amount + b.Amount, a.Currency);
    }

    public static Money operator -(Money a, Money b)
    {
        return a + b.Negate();
    }

    public static Money operator *(Money a, decimal factor)
    {
        return new Money(a.Amount * factor, a.Currency);
    }

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

    public override string ToString()
    {
        return string.Create(CultureInfo.InvariantCulture, $"{Amount:0.##} {Currency}");
    }

    public static void EnsureSameCurrency(Money a, Money b)
    {
        if (!string.Equals(a.Currency, b.Currency, StringComparison.Ordinal))
            throw new InvalidOperationException("Currency mismatch.");
    }
}