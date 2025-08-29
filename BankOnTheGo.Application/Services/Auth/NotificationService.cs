using BankOnTheGo.Application.Interfaces;
using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Services.Auth;

public sealed class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;

    public NotificationService(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task SendPasswordResetAsync(string email, string resetUrl)
    {
        var subject = "Reset your password";
        var body =
            $"Hello,\n\n" +
            $"You requested a password reset. Click the link below to reset your password:\n" +
            $"{resetUrl}\n\n" +
            $"If you didn’t request this, please ignore this email.";

        var message = new Message(new[] { email }, subject, body);
        await _emailService.SendEmail(message);
    }
}