using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankOnTheGo.Domain.Authentication.User;

namespace BankOnTheGo.Application.Interfaces.Auth;

public interface IJwtTokenService
{
    JwtSecurityToken GetToken(List<Claim> authClaims);
    Task<(string AccessToken, RefreshToken RefreshToken)> GenerateTokensAsync(ApplicationUser user, string ipAddress);

    Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(RefreshToken tokenEntity, string? ipAddress);
}