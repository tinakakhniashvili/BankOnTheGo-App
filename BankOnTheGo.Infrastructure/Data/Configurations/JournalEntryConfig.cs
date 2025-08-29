using BankOnTheGo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOnTheGo.Infrastructure.Data.Configurations;

public sealed class JournalEntryConfig : IEntityTypeConfiguration<JournalEntry>
{
    public void Configure(EntityTypeBuilder<JournalEntry> b)
    {
        b.ToTable("JournalEntries");

        b.HasKey(x => x.Id);

        b.Property(x => x.TransactionId)
            .IsRequired();

        b.Property(x => x.Timestamp)
            .IsRequired();

        b.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsRequired();

        b.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey(x => x.JournalEntryId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(x => x.TransactionId);
        b.HasIndex(x => x.Timestamp);
    }
}