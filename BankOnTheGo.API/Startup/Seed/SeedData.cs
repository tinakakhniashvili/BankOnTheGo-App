using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Domain.Models;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.API.Startup.Seed;

public class SeedData
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public SeedData(ApplicationDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    public async Task RunAsync()
    {
        if (await _users.FindByEmailAsync(SeedConstants.AliceEmail) != null)
            return;

        var alice = await CreateUserAsync("Alice Seeds", SeedConstants.AliceEmail, true, false);
        var bob   = await CreateUserAsync("Bob Seeds",   SeedConstants.BobEmail,   true, true);
        var carol = await CreateUserAsync("Carol Seeds", SeedConstants.CarolEmail, false, false);
        var dave  = await CreateUserAsync("Dave Admin",  SeedConstants.DaveEmail,  true, true, true);

        Guid G(string userId) => StableGuid(userId);

        var accAliceUsd = new Account { Type = AccountType.User,     UserId = GGuid(alice.Id), Currency = "USD", Name = "Alice Wallet (USD)" };
        var accAliceEur = new Account { Type = AccountType.User,     UserId = GGuid(alice.Id), Currency = "EUR", Name = "Alice Wallet (EUR)" };
        var accBobUsd   = new Account { Type = AccountType.User,     UserId = GGuid(bob.Id),   Currency = "USD", Name = "Bob Wallet (USD)"   };
        var accBobGel   = new Account { Type = AccountType.User,     UserId = GGuid(bob.Id),   Currency = "GEL", Name = "Bob Wallet (GEL)"   };
        var accFeeUsd   = new Account { Type = AccountType.Fee,      Currency = "USD", Name = "Processing Fees USD"};
        var accFeeGel   = new Account { Type = AccountType.Fee,      Currency = "GEL", Name = "Processing Fees GEL"};
        var accSuspUsd  = new Account { Type = AccountType.Suspense, Currency = "USD", Name = "Settlement (USD)"   };
        var accSuspGel  = new Account { Type = AccountType.Suspense, Currency = "GEL", Name = "Settlement (GEL)"   };

        await _db.Accounts.AddRangeAsync(accAliceUsd, accAliceEur, accBobUsd, accBobGel, accFeeUsd, accFeeGel, accSuspUsd, accSuspGel);

        var wallets = new[]
        {
            new Wallet { UserId = alice.Id, Currency = "USD", Status = WalletStatus.Active },
            new Wallet { UserId = alice.Id, Currency = "EUR", Status = WalletStatus.Active },
            new Wallet { UserId = bob.Id,   Currency = "USD", Status = WalletStatus.Active },
            new Wallet { UserId = bob.Id,   Currency = "GEL", Status = WalletStatus.Active },
            new Wallet { UserId = carol.Id, Currency = "USD", Status = WalletStatus.Locked },
        };
        await _db.Wallets.AddRangeAsync(wallets);

        var now = DateTimeOffset.UtcNow;

        var txTopUpAlice = new Transaction { Type = TransactionType.TopUp, Currency = "USD", Reference = "seed-topup-alice-120", CreatedAt = now.AddDays(-7) };
        txTopUpAlice.TransitionTo(TransactionState.Posted);
        txTopUpAlice.TransitionTo(TransactionState.Settled);
        var jeTopUpAlice = new JournalEntry
        {
            TransactionId = txTopUpAlice.Id,
            Timestamp = txTopUpAlice.CreatedAt,
            Lines = new List<JournalLine>
            {
                JL(accSuspUsd,  M(120.00m, "USD"), EntryDirection.Debit),
                JL(accAliceUsd, M(118.20m, "USD"), EntryDirection.Credit),
                JL(accFeeUsd,   M(  1.80m, "USD"), EntryDirection.Credit),
            }
        };

        var txTransfer = new Transaction { Type = TransactionType.Transfer, Currency = "USD", Reference = "seed-transfer-a2b-50", CreatedAt = now.AddDays(-6) };
        txTransfer.TransitionTo(TransactionState.Posted);
        txTransfer.TransitionTo(TransactionState.Settled);
        var jeTransfer = new JournalEntry
        {
            TransactionId = txTransfer.Id,
            Timestamp = txTransfer.CreatedAt,
            Lines = new List<JournalLine>
            {
                JL(accAliceUsd, M(50.20m, "USD"), EntryDirection.Debit),
                JL(accBobUsd,   M(50.00m, "USD"), EntryDirection.Credit),
                JL(accFeeUsd,   M( 0.20m, "USD"), EntryDirection.Credit),
            }
        };

        var txWithdraw = new Transaction { Type = TransactionType.Withdraw, Currency = "GEL", Reference = "seed-withdraw-bob-50", CreatedAt = now.AddDays(-5) };
        txWithdraw.TransitionTo(TransactionState.Posted);
        txWithdraw.TransitionTo(TransactionState.Settled);
        var jeWithdraw = new JournalEntry
        {
            TransactionId = txWithdraw.Id,
            Timestamp = txWithdraw.CreatedAt,
            Lines = new List<JournalLine>
            {
                JL(accBobGel,  M(50.40m, "GEL"), EntryDirection.Debit),
                JL(accSuspGel, M(50.00m, "GEL"), EntryDirection.Credit),
                JL(accFeeGel,  M( 0.40m, "GEL"), EntryDirection.Credit),
            }
        };

        await _db.Transactions.AddRangeAsync(txTopUpAlice, txTransfer, txWithdraw);
        await _db.JournalEntries.AddRangeAsync(jeTopUpAlice, jeTransfer, jeWithdraw);

        var plPaid = new PaymentLink
        {
            Id = Guid.NewGuid(),
            OwnerUserId  = G(alice.Id),
            Code         = SeedConstants.LinkPaid,
            Currency     = "USD",
            Amount       = M(35.00m, "USD"),
            CreatedAt    = now.AddDays(-2),
            ExpiresAt    = now.AddDays(7),
            Status       = "Paid"
        };
        var plOpen = new PaymentLink
        {
            Id = Guid.NewGuid(),
            OwnerUserId  = G(alice.Id),
            Code         = SeedConstants.LinkOpen,
            Currency     = "USD",
            Amount       = M(49.99m, "USD"),
            CreatedAt    = now.AddDays(-1),
            ExpiresAt    = now.AddDays(3),
            Status       = "Active"
        };
        var plExpired = new PaymentLink
        {
            Id = Guid.NewGuid(),
            OwnerUserId  = G(alice.Id),
            Code         = SeedConstants.LinkExpiry,
            Currency     = "EUR",
            Amount       = M(12.00m, "EUR"),
            CreatedAt    = now.AddDays(-10),
            ExpiresAt    = now.AddDays(-1),
            Status       = "Expired"
        };
        await _db.PaymentLinks.AddRangeAsync(plPaid, plOpen, plExpired);

        await _db.SaveChangesAsync();
    }

    private async Task<ApplicationUser> CreateUserAsync(string name, string email, bool emailConfirmed, bool twoFactor, bool admin = false)
    {
        var user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = emailConfirmed, TwoFactorEnabled = twoFactor };
        var result = await _users.CreateAsync(user, SeedConstants.Password);
        if (!result.Succeeded) throw new Exception($"Failed to create user {email}");
        if (admin) await _users.AddClaimAsync(user, new Claim(ClaimTypes.Role, "Admin"));
        return user;
    }

    private static JournalLine JL(Account account, Money amount, EntryDirection dir) => new JournalLine { AccountId = account.Id, Amount = amount, Direction = dir };
    private static Money M(decimal amount, string currency) => new Money(amount, currency);
    private static Guid? GGuid(string? userId) => string.IsNullOrWhiteSpace(userId) ? null : StableGuid(userId);
    private static Guid StableGuid(string input) { using var md5 = MD5.Create(); return new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(input))); }
}
