using Microsoft.AspNetCore.HttpOverrides;

namespace BankOnTheGo.API.Setup;

public static class ForwardedHeadersSetup
{
    public static void UseForwardedHeadersConfig(this WebApplication app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            RequireHeaderSymmetry = false
        });
    }
}