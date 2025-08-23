using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.Domain.Authentication.User;

public class CreateUserResponse
{
    public string Token { get; set; }
    public IdentityUser User{ get; set; }
}