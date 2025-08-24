namespace BankOnTheGo.Domain.Authentication.Responses;

public record AuthResponse(string Email, string? Token, string? Message);