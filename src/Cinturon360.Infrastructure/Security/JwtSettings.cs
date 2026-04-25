namespace Cinturon360.Infrastructure.Security;

/// <summary>
/// Configuration for JWT token issuance. Bound from appsettings.json "Jwt" section.
/// </summary>
public sealed class JwtSettings
{
    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = "cinturon360";
    public string Audience { get; init; } = "cinturon360-api";
    public int AccessTokenExpiryMinutes { get; init; } = 30;
}
