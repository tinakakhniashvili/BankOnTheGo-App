using System.ComponentModel.DataAnnotations;

namespace BankOnTheGo.Domain.Authentication.SignUp;

public class ResetPassword
{
    [Required] public string Password { get; set; } = null!;

    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string Email { get; set; } = null!;

    public string Token { get; set; } = null!;
}