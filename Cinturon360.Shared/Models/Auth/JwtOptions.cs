namespace Cinturon360.Shared.Models.Auth;

public class JwtOptions
{
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public string SigningKey { get; set; } = default!;
    public int AccessTokenMinutes { get; set; } = 15;  // short-lived
    public int RefreshTokenDays { get; set; } = 30;    // rotate as needed
}
