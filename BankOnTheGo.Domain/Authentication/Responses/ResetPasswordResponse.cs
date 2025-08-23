namespace BankOnTheGo.Service.Models.Authentication.Responses;

public record ResetPasswordResponse(string Email, bool IsReset, string? Message);