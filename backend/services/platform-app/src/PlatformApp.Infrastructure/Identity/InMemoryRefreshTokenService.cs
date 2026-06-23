using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using PlatformApp.Application.Identity;

namespace PlatformApp.Infrastructure.Identity;

/// <summary>
/// In-memory refresh-token store with rotation. For production, replace with the
/// PostgreSQL-backed implementation persisting to identity.refresh_tokens
/// (token, expires_at, revoked_at). Tokens are single-use: a successful refresh
/// revokes the presented token and issues a fresh one.
/// </summary>
public sealed class InMemoryRefreshTokenService : IRefreshTokenService
{
    private sealed record TokenRecord(Guid UserId, string Username, string Role, DateTimeOffset ExpiresAt, bool Revoked);

    private readonly ConcurrentDictionary<string, TokenRecord> _store = new();
    private readonly JwtOptions _options;

    public InMemoryRefreshTokenService(IOptions<JwtOptions> options) => _options = options.Value;

    public Task<string> IssueAsync(Guid userId, string username, string role, CancellationToken cancellationToken)
    {
        var token = GenerateToken();
        _store[token] = new TokenRecord(userId, username, role, DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays), false);
        return Task.FromResult(token);
    }

    public Task<AuthenticateResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (!_store.TryGetValue(refreshToken, out var record)
            || record.Revoked
            || record.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            return Task.FromResult<AuthenticateResponse?>(null);
        }

        // Rotate: revoke the presented token, mint a new pair.
        _store[refreshToken] = record with { Revoked = true };

        var (accessToken, expiresAt) = JwtTokenGenerator.CreateAccessToken(
            _options, record.UserId, record.Username, record.Role);
        var newRefresh = GenerateToken();
        _store[newRefresh] = new TokenRecord(record.UserId, record.Username, record.Role,
            DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays), false);

        return Task.FromResult<AuthenticateResponse?>(new AuthenticateResponse(
            accessToken, expiresAt, newRefresh, record.Username, record.Role));
    }

    public Task RevokeAsync(string refreshToken, CancellationToken cancellationToken)
    {
        if (_store.TryGetValue(refreshToken, out var record))
        {
            _store[refreshToken] = record with { Revoked = true };
        }
        return Task.CompletedTask;
    }

    private static string GenerateToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }
}
