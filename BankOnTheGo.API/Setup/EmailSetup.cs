using BankOnTheGo.Shared.Models;
using NETCore.MailKit.Extensions;
using NETCore.MailKit.Infrastructure.Internal;

namespace BankOnTheGo.API.Setup;

public static class EmailSetup
{
    public static IServiceCollection AddEmailing(this IServiceCollection services, IConfiguration config)
    {
        var emailCfg = config.GetSection("EmailConfiguration").Get<EmailConfiguration>() ?? new EmailConfiguration();
        services.AddSingleton(emailCfg);

        services.AddMailKit(o => o.UseMailKit(new MailKitOptions
        {
            Server = emailCfg.SmtpServer,
            Port   = emailCfg.Port,
            SenderName  = emailCfg.From,
            SenderEmail = emailCfg.From,
            Password    = emailCfg.Password,
            Security    = true
        }));
        return services;
    }
}