using BankOnTheGo.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BankOnTheGo.API.Setup;

public static class DatabaseSetup
{
    public static IServiceCollection AddAppDb(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));
        return services;
    }
}