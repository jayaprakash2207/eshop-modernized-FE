using PlatformApp.Application.Identity;
using PlatformApp.Infrastructure.State;

namespace PlatformApp.Infrastructure.Identity;

public sealed class InMemoryIdentityAccountService : IIdentityAccountService
{
    private readonly AppState _state;

    public InMemoryIdentityAccountService(AppState state)
    {
        _state = state;
    }

    public Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = _state.Users.SingleOrDefault(candidate => candidate.Id == userId);
        return Task.FromResult(user is null ? null : Map(user));
    }

    public Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = _state.Users.SingleOrDefault(candidate => candidate.Id == userId);
        if (user is null)
        {
            return Task.FromResult<UserProfileDto?>(null);
        }

        user.Email = request.Email.Trim();
        user.PhoneNumber = request.PhoneNumber.Trim();
        return Task.FromResult<UserProfileDto?>(Map(user));
    }

    public Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = _state.Users.SingleOrDefault(candidate => candidate.Id == userId);
        if (user is null || !PasswordHasher.VerifyPassword(request.CurrentPassword, user.Password))
        {
            return Task.FromResult(false);
        }

        user.Password = PasswordHasher.HashPassword(request.NewPassword);
        return Task.FromResult(true);
    }

    public Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var user = new AppUser
        {
            Username = request.Username.Trim(),
            Password = PasswordHasher.HashPassword(request.Password),
            Email = request.Email.Trim(),
            PhoneNumber = string.Empty,
            Role = "User",
            EmailConfirmed = false
        };

        _state.Users.Add(user);
        return Task.FromResult(new RegisterResponse(user.Id, user.Username));
    }

    public Task<AccountStatusResponse> ConfirmEmailAsync(string username, CancellationToken cancellationToken)
    {
        var user = _state.Users.SingleOrDefault(candidate => string.Equals(candidate.Username, username, StringComparison.OrdinalIgnoreCase));
        if (user is not null)
        {
            user.EmailConfirmed = true;
        }

        return Task.FromResult(new AccountStatusResponse("Email confirmation processed."));
    }

    public Task<AccountStatusResponse> LogoutAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(new AccountStatusResponse("Logout completed."));

    private static UserProfileDto Map(AppUser user) =>
        new(user.Id, user.Username, user.Email, user.PhoneNumber, user.Role, user.EmailConfirmed);
}
