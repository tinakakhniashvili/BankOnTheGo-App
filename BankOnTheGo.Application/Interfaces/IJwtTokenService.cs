using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BankOnTheGo.Domain.Authentication.User;

namespace BankOnTheGo.Application.Interfaces;

public interface IJwtTokenService
{
    JwtSecurityToken GetToken(List<Claim> authClaims);
    Task<(string AccessToken, RefreshToken RefreshToken)> GenerateTokensAsync(ApplicationUser user, string ipAddress);
}