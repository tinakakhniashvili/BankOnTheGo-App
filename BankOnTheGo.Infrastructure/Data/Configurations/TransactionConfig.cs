using BankOnTheGo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOnTheGo.Infrastructure.Data.Configurations;

public sealed class TransactionConfig : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        b.ToTable("Transactions");

        b.HasKey(x => x.Id);

        b.Property(x => x.Type)
            .HasConversion<string>()
            .IsRequired();

        b.Property(x => x.State)
            .HasConversion<string>()
            .IsRequired();

        b.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.Reference)
            .HasMaxLength(200);

        b.Property(x => x.MetadataJson)
            .HasColumnType("nvarchar(max)");

        b.Property(x => x.CreatedAt).IsRequired();
        b.Property(x => x.UpdatedAt);

        b.HasIndex(x => x.State);
        b.HasIndex(x => new { x.Type, x.Currency });
        b.HasIndex(x => x.Reference);
    }
}