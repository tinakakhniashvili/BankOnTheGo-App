using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Shared.Options;
using Microsoft.Extensions.Options;

namespace BankOnTheGo.Application.Services.Auth;

public sealed class UrlBuilder : IUrlBuilder
{
    private readonly UrlOptions _opt;

    public UrlBuilder(IOptions<UrlOptions> options)
    {
        _opt = options.Value;
    }

    public string BuildPasswordResetUrl(string email, string token)
    {
        var baseUrl = (_opt.BaseUrl ?? string.Empty).TrimEnd('/');
        var path = string.IsNullOrWhiteSpace(_opt.ResetPasswordPath)
            ? "/api/Auth/reset-password"
            : _opt.ResetPasswordPath;
        var e = Uri.EscapeDataString(email ?? string.Empty);
        var t = Uri.EscapeDataString((token ?? string.Empty).Replace(" ", "+"));
        return $"{baseUrl}{path}?token={t}&email={e}";
    }
}