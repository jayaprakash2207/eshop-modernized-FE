namespace PlatformApp.Application.Identity;

public interface IIdentityAccountService
{
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<AccountStatusResponse> ConfirmEmailAsync(string username, CancellationToken cancellationToken);
    Task<AccountStatusResponse> LogoutAsync(Guid userId, CancellationToken cancellationToken);
}
