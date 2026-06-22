namespace PlatformApp.Application.Identity;

public sealed record UserProfileDto(Guid UserId, string Username, string Email, string PhoneNumber, string Role, bool EmailConfirmed);

public sealed record UpdateProfileRequest(string Email, string PhoneNumber);

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public sealed record RegisterRequest(string Username, string Email, string Password);

public sealed record RegisterResponse(Guid UserId, string Username);

public sealed record AccountStatusResponse(string Message);

public sealed record RecoveryCodesResponse(IReadOnlyCollection<string> RecoveryCodes);
