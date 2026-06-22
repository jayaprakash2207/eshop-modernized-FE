using MediatR;
using PlatformApp.Application.Identity.Commands;

namespace PlatformApp.Application.Identity.Handlers;

public sealed class AuthenticateHandler : IRequestHandler<AuthenticateCommand, AuthenticateResponse?>
{
    private readonly IIdentityService _identityService;
    public AuthenticateHandler(IIdentityService identityService) => _identityService = identityService;

    public Task<AuthenticateResponse?> Handle(AuthenticateCommand request, CancellationToken cancellationToken)
        => _identityService.AuthenticateAsync(new AuthenticateRequest(request.Username, request.Password), cancellationToken);
}

public sealed class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IIdentityAccountService _accountService;
    public RegisterHandler(IIdentityAccountService accountService) => _accountService = accountService;

    public Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        => _accountService.RegisterAsync(new RegisterRequest(request.Username, request.Email, request.Password), cancellationToken);
}

public sealed class ChangePasswordHandler : IRequestHandler<ChangePasswordCommand, bool>
{
    private readonly IIdentityAccountService _accountService;
    public ChangePasswordHandler(IIdentityAccountService accountService) => _accountService = accountService;

    public Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        => _accountService.ChangePasswordAsync(request.UserId,
            new ChangePasswordRequest(request.CurrentPassword, request.NewPassword), cancellationToken);
}
