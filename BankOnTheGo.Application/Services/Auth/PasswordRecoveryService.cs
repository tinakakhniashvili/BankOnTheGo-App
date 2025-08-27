using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Domain.Authentication.Responses;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Shared.Models;
using Microsoft.AspNetCore.Identity;

namespace BankOnTheGo.Application.Services.Auth;

public sealed class PasswordRecoveryService : IPasswordRecoveryService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUrlBuilder _urlBuilder;
    private readonly INotificationService _notifications;

    public PasswordRecoveryService(
        UserManager<ApplicationUser> userManager,
        IUrlBuilder urlBuilder,
        INotificationService notifications)
    {
        _userManager = userManager;
        _urlBuilder = urlBuilder;
        _notifications = notifications;
    }

    public async Task<ServiceResult<ResetPasswordResponse>> SendResetLinkAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return ServiceResult<ResetPasswordResponse>.Ok(
                new ResetPasswordResponse(email, false,
                    "If an account with this email exists, a password reset link has been sent."));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = _urlBuilder.BuildPasswordResetUrl(email, token);

        await _notifications.SendPasswordResetAsync(email, url);

        return ServiceResult<ResetPasswordResponse>.Ok(
            new ResetPasswordResponse(email, false, $"Password reset link sent to {email}."));
    }

    public async Task<ServiceResult<ResetPasswordResponse>> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return ServiceResult<ResetPasswordResponse>.Fail("User not found.");

        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        if (!result.Succeeded)
        {
            var err = string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return ServiceResult<ResetPasswordResponse>.Fail(err);
        }

        return ServiceResult<ResetPasswordResponse>.Ok(
            new ResetPasswordResponse(email, true, "Password has been reset successfully."));
    }
}
