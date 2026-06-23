using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PlatformApp.Infrastructure.Identity;

/// <summary>
/// Shared JWT access-token factory used by both first-time authentication and
/// refresh-token rotation, so both paths produce identically-shaped tokens.
/// </summary>
public static class JwtTokenGenerator
{
    public static (string AccessToken, DateTimeOffset ExpiresAt) CreateAccessToken(
        JwtOptions options, Guid userId, string username, string role)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(options.AccessTokenMinutes);
        var key = Encoding.UTF8.GetBytes(options.SigningKey);
        var handler = new JwtSecurityTokenHandler();

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(ClaimTypes.Name, username),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Audience            = options.Audience,
            Issuer              = options.Issuer,
            Subject             = new ClaimsIdentity(claims),
            Expires             = expiresAt.UtcDateTime,
            SigningCredentials  = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = handler.CreateToken(descriptor);
        return (handler.WriteToken(token), expiresAt);
    }
}
