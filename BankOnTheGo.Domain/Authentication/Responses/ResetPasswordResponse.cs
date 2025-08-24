namespace BankOnTheGo.Domain.Authentication.Responses;

public record ResetPasswordResponse(string Email, bool IsReset, string? Message);