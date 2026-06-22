namespace PlatformApp.Application.Identity;

public sealed record AuthenticateRequest(string Username, string Password);

public sealed record AuthenticateResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string RefreshToken,
    string Username,
    string Role);

public interface IIdentityService
{
    Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken);
}
