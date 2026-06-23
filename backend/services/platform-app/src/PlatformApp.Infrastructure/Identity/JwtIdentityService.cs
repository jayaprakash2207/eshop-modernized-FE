using Microsoft.Extensions.Options;
using PlatformApp.Application.Identity;
using PlatformApp.Infrastructure.State;

namespace PlatformApp.Infrastructure.Identity;

public sealed class JwtIdentityService : IIdentityService
{
    private readonly JwtOptions _options;
    private readonly AppState _state;
    private readonly IRefreshTokenService _refreshTokens;

    public JwtIdentityService(IOptions<JwtOptions> options, AppState state, IRefreshTokenService refreshTokens)
    {
        _options = options.Value;
        _state = state;
        _refreshTokens = refreshTokens;
    }

    public async Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        var user = _state.Users.SingleOrDefault(candidate =>
            string.Equals(candidate.Username, request.Username.Trim(), StringComparison.OrdinalIgnoreCase));

        if (user is null || !PasswordHasher.VerifyPassword(request.Password, user.Password))
        {
            return null;
        }

        var (accessToken, expiresAt) = JwtTokenGenerator.CreateAccessToken(_options, user.Id, user.Username, user.Role);
        var refreshToken = await _refreshTokens.IssueAsync(user.Id, user.Username, user.Role, cancellationToken);

        return new AuthenticateResponse(accessToken, expiresAt, refreshToken, user.Username, user.Role);
    }
}
