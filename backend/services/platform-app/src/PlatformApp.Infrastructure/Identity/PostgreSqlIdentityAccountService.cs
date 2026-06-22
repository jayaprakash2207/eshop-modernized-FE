using Dapper;
using Microsoft.Extensions.Options;
using PlatformApp.Application.Identity;
using PlatformApp.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace PlatformApp.Infrastructure.Identity;

public sealed class PostgreSqlIdentityAccountService : IIdentityService, IIdentityAccountService
{
    private readonly PostgreSqlConnectionFactory _connectionFactory;
    private readonly JwtOptions _jwtOptions;

    public PostgreSqlIdentityAccountService(PostgreSqlConnectionFactory connectionFactory, IOptions<JwtOptions> jwtOptions)
    {
        _connectionFactory = connectionFactory;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, username, role, password_hash
            FROM identity.user_profiles
            WHERE username = @Username;
            """;

        await using var connection = _connectionFactory.CreateConnection();
        var user = await connection.QuerySingleOrDefaultAsync<AuthUserRow>(new CommandDefinition(sql, new { Username = request.Username.Trim() }, cancellationToken: cancellationToken));
        if (user is null || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtOptions.SigningKey);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Username),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Audience = _jwtOptions.Audience,
            Issuer = _jwtOptions.Issuer,
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        var token = tokenHandler.CreateToken(descriptor);
        return new AuthenticateResponse(tokenHandler.WriteToken(token), expiresAt, Convert.ToBase64String(Guid.NewGuid().ToByteArray()), user.Username, user.Role);
    }

    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id, username, email, phone_number, role, email_confirmed
            FROM identity.user_profiles
            WHERE id = @userId;
            """;
        await using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(new CommandDefinition(sql, new { userId }, cancellationToken: cancellationToken));
        return row is null ? null : Map(row);
    }

    public async Task<UserProfileDto?> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE identity.user_profiles
            SET email = @Email, phone_number = @PhoneNumber, updated_at = NOW()
            WHERE id = @UserId;
            """;
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, Email = request.Email.Trim(), PhoneNumber = request.PhoneNumber.Trim() }, cancellationToken: cancellationToken));
        return await GetProfileAsync(userId, cancellationToken);
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        const string checkSql = "SELECT password_hash FROM identity.user_profiles WHERE id = @userId;";
        const string updateSql = "UPDATE identity.user_profiles SET password_hash = @Password, updated_at = NOW() WHERE id = @userId;";
        await using var connection = _connectionFactory.CreateConnection();
        var current = await connection.ExecuteScalarAsync<string?>(new CommandDefinition(checkSql, new { userId }, cancellationToken: cancellationToken));
        if (current is null || !PasswordHasher.VerifyPassword(request.CurrentPassword, current))
        {
            return false;
        }

        var hashedPassword = PasswordHasher.HashPassword(request.NewPassword);
        await connection.ExecuteAsync(new CommandDefinition(updateSql, new { userId, Password = hashedPassword }, cancellationToken: cancellationToken));
        return true;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO identity.user_profiles
            (id, username, email, phone_number, role, refresh_token_hash, password_hash, email_confirmed, created_at, updated_at)
            VALUES
            (@Id, @Username, @Email, '', 'User', NULL, @PasswordHash, FALSE, NOW(), NOW());
            """;

        var id = Guid.NewGuid();
        var hashedPassword = PasswordHasher.HashPassword(request.Password);
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, Username = request.Username.Trim(), Email = request.Email.Trim(), PasswordHash = hashedPassword }, cancellationToken: cancellationToken));
        return new RegisterResponse(id, request.Username.Trim());
    }

    public async Task<AccountStatusResponse> ConfirmEmailAsync(string username, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE identity.user_profiles SET email_confirmed = TRUE, updated_at = NOW() WHERE username = @username;";
        await using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(new CommandDefinition(sql, new { username }, cancellationToken: cancellationToken));
        return new AccountStatusResponse("Email confirmation processed.");
    }

    public Task<AccountStatusResponse> LogoutAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(new AccountStatusResponse("Logout completed."));

    private static UserProfileDto Map(UserRow row) => new(row.Id, row.Username, row.Email, row.PhoneNumber, row.Role, row.EmailConfirmed);

    private sealed record AuthUserRow(Guid Id, string Username, string Role, string PasswordHash);
    private sealed record UserRow(Guid Id, string Username, string Email, string PhoneNumber, string Role, bool EmailConfirmed);
}
