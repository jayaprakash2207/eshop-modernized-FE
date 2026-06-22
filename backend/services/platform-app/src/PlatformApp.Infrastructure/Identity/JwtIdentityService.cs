using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlatformApp.Application.Identity;
using PlatformApp.Infrastructure.State;

namespace PlatformApp.Infrastructure.Identity;

public sealed class JwtIdentityService : IIdentityService
{
    private readonly JwtOptions _options;
    private readonly AppState _state;

    public JwtIdentityService(IOptions<JwtOptions> options, AppState state)
    {
        _options = options.Value;
        _state = state;
    }

    public Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        var user = _state.Users.SingleOrDefault(candidate => string.Equals(candidate.Username, request.Username.Trim(), StringComparison.OrdinalIgnoreCase));
        if (user is null || !PasswordHasher.VerifyPassword(request.Password, user.Password))
        {
            return Task.FromResult<AuthenticateResponse?>(null);
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SigningKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.Username),
            new(JwtRegisteredClaimNames.UniqueName, request.Username),
            new(ClaimTypes.Name, request.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Audience = _options.Audience,
            Issuer = _options.Issuer,
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(descriptor);
        var accessToken = tokenHandler.WriteToken(token);
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        return Task.FromResult<AuthenticateResponse?>(new AuthenticateResponse(
            accessToken,
            expiresAt,
            refreshToken,
            user.Username,
            user.Role));
    }
}
