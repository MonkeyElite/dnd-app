using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DndApp.Identity.Data.Entities;
using DndApp.Identity.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

namespace DndApp.Identity.Security;

public sealed class JwtTokenService(IOptions<AuthOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _jwt = options.Value.Jwt;

    public string CreateAccessToken(User user)
    {
        if (string.IsNullOrWhiteSpace(_jwt.SigningKey))
        {
            throw new InvalidOperationException("Auth:Jwt:SigningKey must be configured.");
        }

        var expires = DateTime.UtcNow.AddHours(_jwt.AccessTokenHours <= 0 ? 12 : _jwt.AccessTokenHours);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim("username", user.Username),
            new Claim("displayName", user.DisplayName),
            new Claim("isPlatformAdmin", user.IsPlatformAdmin ? "true" : "false")
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}
