namespace BankOnTheGo.Service.Models.Authentication.Responses;

public record ConfirmEmailResponse(string Email, bool IsConfirmed);