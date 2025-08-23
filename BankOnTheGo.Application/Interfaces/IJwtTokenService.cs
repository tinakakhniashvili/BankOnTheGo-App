using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankOnTheGo.Application.Interfaces;

public interface IJwtTokenService
{
    JwtSecurityToken GetToken(List<Claim> authClaims);
}