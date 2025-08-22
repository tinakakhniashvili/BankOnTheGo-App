namespace User.Management.Service.Models.Authentication.Responses;

public record ConfirmEmailResponse(string Email, bool IsConfirmed);