using MediatR;

namespace PlatformApp.Application.Identity.Commands;

public record AuthenticateCommand(string Username, string Password) : IRequest<AuthenticateResponse?>;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<RegisterResponse>;

public record ChangePasswordCommand(Guid UserId, string CurrentPassword, string NewPassword) : IRequest<bool>;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthenticateResponse?>;
