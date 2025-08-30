using System.Security.Claims;
using System.Text;
using BankOnTheGo.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace BankOnTheGo.API.Setup;

public static class AuthSetup
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection("Jwt"));

        services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwt = config.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
                var key = string.IsNullOrWhiteSpace(jwt.Key) ? config["JWT:Secret"] : jwt.Key;
                var issuer = string.IsNullOrWhiteSpace(jwt.Issuer) ? config["JWT:ValidIssuer"] : jwt.Issuer;
                var audience = string.IsNullOrWhiteSpace(jwt.Audience) ? config["JWT:ValidAudience"] : jwt.Audience;

                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key ?? string.Empty)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.Name
                };
            });

        return services;
    }
}