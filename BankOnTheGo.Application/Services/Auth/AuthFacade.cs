using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.SignUp;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Services.Auth
{
    public sealed class AuthFacade : IAuthFacade
    {
        private readonly IRegistrationService _registration;
        private readonly IAuthenticationService _authentication;
        private readonly IPasswordRecoveryService _passwords;

        public AuthFacade(
            IRegistrationService registration,
            IAuthenticationService authentication,
            IPasswordRecoveryService passwords)
        {
            _registration = registration;
            _authentication = authentication;
            _passwords = passwords;
        }

        public Task<ServiceResult<string>> RegisterAsync(RegisterUser input, string role)
            => _registration.RegisterAsync(input, role);

        public Task<ServiceResult<ConfirmEmailResponse>> ConfirmEmailAsync(string token, string email)
            => _registration.ConfirmEmailAsync(token, email);

        public Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel input)
            => _authentication.LoginAsync(input);

        public Task<ServiceResult<ResetPasswordResponse>> SendResetAsync(string email)
            => _passwords.SendResetLinkAsync(email);

        public Task<ServiceResult<ResetPasswordResponse>> ResetAsync(string email, string token, string newPassword)
            => _passwords.ResetPasswordAsync(email, token, newPassword);
    }
}