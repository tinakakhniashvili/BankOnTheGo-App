using BankOnTheGo.Shared.Models;

namespace BankOnTheGo.Application.Interfaces;

public interface IEmailService
{
    Task SendEmail(Message message);
}