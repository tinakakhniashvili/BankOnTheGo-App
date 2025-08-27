namespace BankOnTheGo.Application.Interfaces.Auth
{
    public interface IUrlBuilder
    {
        string BuildPasswordResetUrl(string email, string token);
    }
}
