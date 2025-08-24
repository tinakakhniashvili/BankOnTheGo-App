namespace BankOnTheGo.Domain.Authentication.Responses;

public record ConfirmEmailResponse(string Email, bool IsConfirmed);