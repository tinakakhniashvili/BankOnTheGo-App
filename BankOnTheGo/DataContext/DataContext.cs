using BankOnTheGo.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BankOnTheGo.DataContext
{
    public class DataContext : DbContext
    {
        public DbSet<RegisterModel> registers { get; set; }
        public DbSet<LoginModel> logins { get; set; }

        public DataContext(DbContextOptions options) : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LoginModel>()
            .HasKey(e => e.Id);

            base.OnModelCreating(modelBuilder);
        }
    }
}
