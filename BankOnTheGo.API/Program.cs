using System.Text;
using System.Security.Claims;
using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Application.Services;
using BankOnTheGo.Application.Services.Auth;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Infrastructure.Data;
using BankOnTheGo.Infrastructure.Repositories;
using BankOnTheGo.Infrastructure.Services;
using BankOnTheGo.Shared.Models;
using BankOnTheGo.Shared.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;
using AuthenticationService = BankOnTheGo.Application.Services.Auth.AuthenticationService;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

builder.Services
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

builder.Services.Configure<DataProtectionTokenProviderOptions>(opts =>
    opts.TokenLifespan = TimeSpan.FromMinutes(10));

builder.Services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
builder.Services.Configure<UrlOptions>(configuration.GetSection("UrlOptions"));

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwt = configuration.GetSection("Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var key = string.IsNullOrWhiteSpace(jwt.Key) ? configuration["JWT:Secret"] : jwt.Key;
        var issuer = string.IsNullOrWhiteSpace(jwt.Issuer) ? configuration["JWT:ValidIssuer"] : jwt.Issuer;
        var audience = string.IsNullOrWhiteSpace(jwt.Audience) ? configuration["JWT:ValidAudience"] : jwt.Audience;

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

var emailConfig = configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>() ?? new EmailConfiguration();
builder.Services.AddSingleton(emailConfig);

builder.Services.AddMailKit(config => config.UseMailKit(new MailKitOptions
{
    Server = emailConfig.SmtpServer,
    Port = emailConfig.Port,
    SenderName = emailConfig.From,
    SenderEmail = emailConfig.From,
    Password = emailConfig.Password,
    Security = true
}));

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IAuthFacade, AuthFacade>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IRegistrationService, RegistrationService>();
builder.Services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUrlBuilder, UrlBuilder>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiExceptionFilter>();
    options.Filters.Add<ValidateModelFilter>();
    options.Filters.Add<LoggingFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "BankOnTheGo", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter a valid JWT token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownNetworks = { },
    KnownProxies = { },
    RequireHeaderSymmetry = false
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.RoutePrefix = string.Empty);
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
