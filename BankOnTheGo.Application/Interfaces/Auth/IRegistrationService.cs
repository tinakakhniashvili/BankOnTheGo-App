using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.SignUp;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Interfaces.Auth;

public interface IRegistrationService
{
    Task<ServiceResult<string>> RegisterAsync(RegisterUser input, string role);
    Task<ServiceResult<ConfirmEmailResponse>> ConfirmEmailAsync(string token, string email);
}