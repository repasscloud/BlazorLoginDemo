namespace Cinturon360.Application.Abstractions.Security;

/// <summary>
/// Token generation and validation service — implemented in Infrastructure.
/// </summary>
public interface ITokenService
{
    /// <summary>Issues a short-lived JWT access token for a user session.</summary>
    string GenerateAccessToken(string userId, string? orgId, string jti, IReadOnlyList<string> permissions);

    /// <summary>Validates a PAT or service account token hash against the stored hash.</summary>
    bool ValidateTokenHash(string rawToken, string storedHash);

    /// <summary>Computes SHA-256 hash of the raw token for storage.</summary>
    string HashToken(string rawToken);

    /// <summary>Generates a raw PAT string (shown once) and returns it with its prefix and hash.</summary>
    (string RawToken, string Prefix, string Hash) GeneratePat(string prefix = "c360pat");
}
