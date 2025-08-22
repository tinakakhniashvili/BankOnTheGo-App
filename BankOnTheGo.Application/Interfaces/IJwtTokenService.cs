using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BankOnTheGo.Service.Services.Interfaces;

public interface IJwtTokenService
{
    JwtSecurityToken GetToken(List<Claim> authClaims);
}