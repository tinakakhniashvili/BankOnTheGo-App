using BankOnTheGo.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankOnTheGo.Infrastructure.Data.Configurations;

public sealed class AccountConfig : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        b.ToTable("Accounts");

        b.HasKey(x => x.Id);

        b.Property(x => x.Type)
            .HasConversion<string>()               
            .IsRequired();

        b.Property(x => x.UserId);

        b.Property(x => x.Currency)
            .HasMaxLength(3)
            .IsUnicode(false)
            .IsRequired();

        b.Property(x => x.Name)
            .HasMaxLength(100);

        b.Property(x => x.IsActive)
            .IsRequired();

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.HasIndex(x => new { x.UserId, x.Currency }); 
        b.HasIndex(x => x.Type);
    }
}