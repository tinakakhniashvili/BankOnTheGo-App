using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace User.Management.Service.Services.Interfaces;

public interface IJwtTokenService
{
    JwtSecurityToken GetToken(List<Claim> authClaims);
}