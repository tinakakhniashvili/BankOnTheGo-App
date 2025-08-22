using BankOnTheGo.Service.Models;

namespace BankOnTheGo.Service.Services.Interfaces;

public interface IEmailService
{
    Task SendEmail(Message message);
}