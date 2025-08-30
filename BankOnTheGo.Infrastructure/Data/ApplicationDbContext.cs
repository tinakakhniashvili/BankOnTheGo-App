using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Domain.Models;
using BankOnTheGo.Infrastructure.Data.Configurations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Wallet> Wallets { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<JournalLine> JournalLines { get; set; }
    public DbSet<PaymentLink> PaymentLinks { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new AccountConfig());
        builder.ApplyConfiguration(new TransactionConfig());
        builder.ApplyConfiguration(new JournalEntryConfig());
        builder.ApplyConfiguration(new JournalLineConfig());
        builder.ApplyConfiguration(new PaymentLinkConfig());

        base.OnModelCreating(builder);

        builder.Entity<RefreshToken>(e =>
        {
            e.HasKey(rt => rt.Id);

            e.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Wallet>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.HasIndex(x => new { x.UserId, x.Currency }).IsUnique();
            e.Property(x => x.Status).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();

            e.HasOne(x => x.User)
                .WithMany(u => u.Wallets)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        builder.Entity<LedgerEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.AmountMinor).IsRequired();
            e.Property(x => x.Currency).HasMaxLength(3).IsRequired();
            e.Property(x => x.Type).IsRequired();
            e.Property(x => x.CreatedAtUtc).IsRequired();
            e.Property(x => x.CorrelationId).HasMaxLength(64);
            e.HasIndex(x => x.CorrelationId);

            e.HasOne(x => x.Wallet)
                .WithMany(w => w.Ledger)
                .HasForeignKey(x => x.WalletId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        SeedRoles(builder);
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "11111111-1111-1111-1111-111111111111",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "11111111-1111-1111-1111-111111111111"
            },
            new IdentityRole
            {
                Id = "22222222-2222-2222-2222-222222222222",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "22222222-2222-2222-2222-222222222222"
            });
    }
}