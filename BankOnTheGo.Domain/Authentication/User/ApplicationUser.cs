using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.Domain.Authentication.User;

public class ApplicationUser : IdentityUser
{
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}