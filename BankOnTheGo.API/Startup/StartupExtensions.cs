using Microsoft.EntityFrameworkCore;
using BankOnTheGo.Infrastructure.Data;

namespace BankOnTheGo.API.Startup;

public static class StartupExtensions
{
    public static async Task UseMigrationsAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var db  = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        await db.Database.MigrateAsync();

        if (env.IsDevelopment())
        {
            var seeder = scope.ServiceProvider.GetRequiredService<Seed.SeedData>();
            await seeder.RunAsync();
        }
    }
}