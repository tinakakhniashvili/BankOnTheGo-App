using BankOnTheGo.Domain.Authentication.Login;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Interfaces.Auth;

public interface IAuthenticationService
{
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginModel input);
}