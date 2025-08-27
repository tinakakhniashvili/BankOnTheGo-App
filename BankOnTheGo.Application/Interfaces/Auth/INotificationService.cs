namespace BankOnTheGo.Application.Interfaces.Auth;

public interface INotificationService
{
    Task SendPasswordResetAsync(string email, string resetUrl);
}

