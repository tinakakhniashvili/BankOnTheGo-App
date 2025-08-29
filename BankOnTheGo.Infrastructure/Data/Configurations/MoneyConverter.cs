using System.Globalization;
using BankOnTheGo.Domain.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BankOnTheGo.Infrastructure.Data.Configurations;

public sealed class MoneyConverter : ValueConverter<Money, string>
{
    public static readonly MoneyConverter Instance = new();

    private MoneyConverter()
        : base(
            v => Serialize(v),
            s => Deserialize(s))
    { }

    public static readonly ValueComparer<Money> Comparer = new(
        (a, b) => a.Amount == b.Amount && string.Equals(a.Currency, b.Currency, StringComparison.Ordinal),
        v => HashCode.Combine(v.Amount, v.Currency),
        v => new Money(v.Amount, v.Currency)
    );

    private static string Serialize(Money v)
        => string.Create(CultureInfo.InvariantCulture, $"{v.Currency}:{v.Amount:0.################}");

    private static Money Deserialize(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new InvalidOperationException("Invalid Money payload: empty.");

        var idx = s.IndexOf(':');
        if (idx <= 0 || idx == s.Length - 1)
            throw new InvalidOperationException($"Invalid Money payload: '{s}'.");

        var currency = s.Substring(0, idx).ToUpperInvariant();
        var amountStr = s[(idx + 1)..];

        if (!decimal.TryParse(amountStr, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount))
            throw new InvalidOperationException($"Invalid Money amount: '{amountStr}'.");

        return new Money(amount, currency);
    }
}