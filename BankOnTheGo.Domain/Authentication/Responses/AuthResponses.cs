namespace BankOnTheGo.Service.Models.Authentication.Responses;

public record AuthResponse(string Email, string? Token, string? Message);