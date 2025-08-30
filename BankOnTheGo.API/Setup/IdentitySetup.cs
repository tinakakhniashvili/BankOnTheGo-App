using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.API.Setup;

public static class IdentitySetup
{
    public static IServiceCollection AddAppIdentity(this IServiceCollection services, IConfiguration _)
    {
        services
            .AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<EmailTokenProvider<ApplicationUser>>("Email");

        services.Configure<DataProtectionTokenProviderOptions>(o =>
            o.TokenLifespan = TimeSpan.FromMinutes(10));

        return services;
    }
}