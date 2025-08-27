using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.SignUp;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Interfaces.Auth
{
    public interface IAuthFacade
    {
        Task<ServiceResult<string>> RegisterAsync(RegisterUser input, string role);
        Task<ServiceResult<ConfirmEmailResponse>> ConfirmEmailAsync(string token, string email);
        Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel input);
        Task<ServiceResult<ResetPasswordResponse>> SendResetAsync(string email);
        Task<ServiceResult<ResetPasswordResponse>> ResetAsync(string email, string token, string newPassword);
    }
}