using MediatR;

namespace PlatformApp.Application.Identity.Commands;

public record AuthenticateCommand(string Username, string Password) : IRequest<AuthenticateResponse?>;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<RegisterResponse>;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;

// NOTE: RefreshTokenCommand intentionally omitted until the refresh-token flow
// (issue + rotate + revoke) is implemented in Wave 1. Declaring a command with no
// handler would throw at runtime. See docs/GAP_ANALYSIS.md P2 item 11.
