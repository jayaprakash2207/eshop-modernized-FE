namespace PlatformApp.Application.Identity;

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record RevokeTokenRequest(string RefreshToken);

/// <summary>
/// Issues, rotates, and revokes refresh tokens. A refresh token is single-use:
/// presenting one issues a new access token AND a new refresh token, and revokes
/// the presented one (rotation). Revoked or expired tokens are rejected.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>Issue a new refresh token for a freshly authenticated user.</summary>
    Task<string> IssueAsync(Guid userId, string username, string role, CancellationToken cancellationToken);

    /// <summary>Validate + rotate a refresh token, returning a new access+refresh pair, or null if invalid.</summary>
    Task<AuthenticateResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Revoke a refresh token (logout / security event).</summary>
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken);
}
