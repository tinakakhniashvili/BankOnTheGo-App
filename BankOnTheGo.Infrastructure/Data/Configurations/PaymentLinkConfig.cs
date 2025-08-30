using BankOnTheGo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOnTheGo.Infrastructure.Data.Configurations;

public sealed class PaymentLinkConfig : IEntityTypeConfiguration<PaymentLink>
{
    public void Configure(EntityTypeBuilder<PaymentLink> b)
    {
        b.ToTable("PaymentLinks");

        b.HasKey(x => x.Id);
        b.Property(x => x.Id).ValueGeneratedOnAdd();

        b.Property(x => x.OwnerUserId).IsRequired();

        b.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(64);

        b.HasIndex(x => x.Code).IsUnique();
     
        b.OwnsOne<Money>(pl => pl.Amount, owned =>
        {
            owned.Property(m => m.Amount)
                .HasColumnName("Amount")
                .HasPrecision(18, 2)
                .IsRequired();

            owned.Property(m => m.Currency)
                .HasColumnName("Currency")
                .HasMaxLength(3)
                .IsRequired();
        });

        b.Property(x => x.Memo)
            .HasMaxLength(512);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.ExpiresAt);

        b.Property(x => x.Status)
            .IsRequired()
            .HasMaxLength(32);
    }
}