using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BankOnTheGo.Application.Interfaces.Auth;
using BankOnTheGo.Domain.Authentication.User;
using BankOnTheGo.Infrastructure.Data;
using BankOnTheGo.Shared.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BankOnTheGo.Infrastructure.Services;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;
    private readonly JwtOptions _jwt;
    private readonly UserManager<ApplicationUser> _userManager;

    public JwtTokenService(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
        IOptions<JwtOptions> jwtOptions, IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _jwt = jwtOptions.Value;
        _configuration = configuration;
    }

    public JwtSecurityToken GetToken(List<Claim> claims)
    {
        var eff = GetEffectiveJwt();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(eff.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes <= 0 ? 60 : _jwt.AccessTokenMinutes);

        return new JwtSecurityToken(
            eff.Issuer,
            eff.Audience,
            claims,
            DateTime.UtcNow,
            expires,
            creds
        );
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> GenerateTokensAsync(ApplicationUser user,
        string ipAddress)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var jwt = GetToken(claims);
        var access = new JwtSecurityTokenHandler().WriteToken(jwt);

        var refresh = new RefreshToken
        {
            Token = Guid.NewGuid().ToString("N"),
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays <= 0 ? 7 : _jwt.RefreshTokenDays),
            CreatedByIp = ipAddress,
            UserId = user.Id
        };

        _context.RefreshTokens.Add(refresh);
        await _context.SaveChangesAsync();

        return (access, refresh);
    }

    public async Task<(string AccessToken, RefreshToken RefreshToken)> RefreshTokenAsync(RefreshToken tokenEntity,
        string? ipAddress)
    {
        tokenEntity.Revoked = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;

        var user = tokenEntity.User;
        var newTokens = await GenerateTokensAsync(user, ipAddress ?? "unknown");
        tokenEntity.ReplacedByToken = newTokens.RefreshToken.Token;

        await _context.SaveChangesAsync();
        return newTokens;
    }

    private (string Key, string Issuer, string Audience) GetEffectiveJwt()
    {
        var key = string.IsNullOrWhiteSpace(_jwt.Key) ? _configuration["JWT:Secret"] ?? "" : _jwt.Key;
        var issuer = string.IsNullOrWhiteSpace(_jwt.Issuer) ? _configuration["JWT:ValidIssuer"] ?? "" : _jwt.Issuer;
        var audience = string.IsNullOrWhiteSpace(_jwt.Audience)
            ? _configuration["JWT:ValidAudience"] ?? ""
            : _jwt.Audience;
        return (key, issuer, audience);
    }
}