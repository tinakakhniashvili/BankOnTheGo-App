using User.Management.Service.Models;
using User.Management.Service.Models.Authentication.Login;
using User.Management.Service.Models.Authentication.Responses;
using User.Management.Service.Models.Authentication.SignUp;

namespace User.Management.Service.Services.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<string>> RegisterAsync(RegisterUser registerUser, string role);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel loginModel);
    Task<ServiceResult<ConfirmEmailResponse>> ConfirmEmailAsync(string token, string email);
    Task<ServiceResult<ResetPasswordResponse>> ForgotPasswordAsync(string email, string baseUrl);
    Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(string email, string token, string newPassword);
    Task<ServiceResult<AuthResponse>> LoginWithOtpAsync(string code, string username);
}