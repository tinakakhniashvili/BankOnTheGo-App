using BankOnTheGo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOnTheGo.Infrastructure.Data.Configurations;

public sealed class JournalLineConfig : IEntityTypeConfiguration<JournalLine>
{
    public void Configure(EntityTypeBuilder<JournalLine> b)
    {
        b.ToTable("JournalLines");

        b.HasKey(x => x.Id);

        b.Property(x => x.JournalEntryId).IsRequired();
        b.Property(x => x.AccountId).IsRequired();

        b.Property(x => x.Direction)
            .HasConversion<string>()
            .IsRequired();

        var moneyProp = b.Property(x => x.Amount)
            .HasConversion(MoneyConverter.Instance)
            .HasColumnName("Money")
            .IsUnicode(false)
            .HasMaxLength(64)
            .IsRequired();

        moneyProp.Metadata.SetValueComparer(MoneyConverter.Comparer);

        b.HasIndex(x => x.AccountId);
        b.HasIndex(x => x.JournalEntryId);
        b.HasIndex(x => new { x.Direction, x.AccountId });
    }
}