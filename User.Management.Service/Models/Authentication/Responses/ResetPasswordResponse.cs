namespace User.Management.Service.Models.Authentication.Responses;

public record ResetPasswordResponse(string Email, bool IsReset, string? Message);