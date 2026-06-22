namespace PlatformApp.Infrastructure.Identity;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "PlatformApp";
    public string Audience { get; init; } = "PlatformApp.Client";
    public string SigningKey { get; init; } = "local-development-signing-key-change-me";
    public int AccessTokenMinutes { get; init; } = 60;
    public int RefreshTokenDays { get; init; } = 7;
}
