using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Interfaces.Auth;

public interface IPasswordRecoveryService
{
    Task<ServiceResult<ResetPasswordResponse>> SendResetLinkAsync(string email);
    Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(string email, string token, string newPassword);
}