using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Application.Interfaces.Ledger;
using BankOnTheGo.Application.Interfaces.Repositories;
using BankOnTheGo.Application.Interfaces.Wallet;
using BankOnTheGo.Application.Services;
using BankOnTheGo.Application.Services.Auth;
using BankOnTheGo.Application.Services.Ledger;
using BankOnTheGo.Application.Services.Wallet;
using BankOnTheGo.Infrastructure.Repositories;
using BankOnTheGo.Infrastructure.Services;

namespace BankOnTheGo.API.Setup;

public static class AppServicesSetup
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ILedgerRepository, LedgerRepository>();
        services.AddScoped<IPaymentLinkRepository, PaymentLinkRepository>();

        // Domain services
        services.AddScoped<ILedgerService, LedgerService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IWalletService, WalletFacade>();
        services.AddScoped<IAuthFacade, AuthFacade>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IPasswordRecoveryService, PasswordRecoveryService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUrlBuilder, UrlBuilder>();
        services.AddScoped<IWalletManagementService, WalletManagementService>();
        services.AddScoped<ITransferService, TransferService>();
        services.AddScoped<IPayoutService, PayoutService>();
        services.AddScoped<IPaymentLinkService, PaymentLinkService>();
        services.AddScoped<IIdempotencyExecutor, IdempotencyExecutor>();

        // Singleton infra
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        return services;
    }
}
