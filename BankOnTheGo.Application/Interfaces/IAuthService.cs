using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.SignUp;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<string>> RegisterAsync(RegisterUser registerUser, string role);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel loginModel);
    Task<ServiceResult<ConfirmEmailResponse>> ConfirmEmailAsync(string token, string email);
    Task<ServiceResult<ResetPasswordResponse>> ForgotPasswordAsync(string email, string baseUrl);
    Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(string email, string token, string newPassword);
    Task SendConfirmationEmailAsync(string email, string token, string baseUrl);
    Task<object> GenerateJwtForUserAsync(ApplicationUser user); // <-- Changed
}