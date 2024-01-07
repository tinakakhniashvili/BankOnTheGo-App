using BankOnTheGo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BankOnTheGo.Data
{
    public class DataContext : DbContext
    {
        public DbSet<UserModel> Users { get; set; }
        public DbSet<WalletModel> Wallets { get; set; }
        public DbSet<TemporaryCodes> TemporaryCodes { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>()
                 .HasKey(e => e.Id);

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<WalletModel>()
                   .HasKey(e => e.Id);

            modelBuilder.Entity<TemporaryCodes>()
                .HasKey(e => e.Id);
        }
    }
}
