using User.Management.Service.Models;

namespace User.Management.Service.Services.Interfaces;

public interface IEmailService
{
    void SendEmail(Message message);
}