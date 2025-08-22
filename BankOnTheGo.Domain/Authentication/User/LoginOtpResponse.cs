using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.Domain.Authentication.User;

public class LoginOtpResponse
{
    public string Token { get; set; } = null!;
    public bool IsTwoFactorEnable { get; set; }
    public IdentityUser User { get; set; } = null!;
}