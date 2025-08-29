using System.Security.Claims;

namespace BankOnTheGo.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? user.FindFirstValue("sub")
        ?? user.FindFirstValue("uid");
}