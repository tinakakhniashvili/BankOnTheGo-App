namespace BankOnTheGo.Shared.Options
{
    public sealed class UrlOptions
    {
        public string BaseUrl { get; set; } = "https://localhost:7275";
        public string ResetPasswordPath { get; set; } = "/api/Auth/reset-password";
        public string ConfirmEmailPath { get; set; } = "/api/Auth/confirm-email";
    }
}