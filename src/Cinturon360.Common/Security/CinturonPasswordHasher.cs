using System.Security.Cryptography;
using System.Text;

namespace Cinturon360.Common.Security;

/// <summary>
/// App-compatible PBKDF2 password hasher for user_security.password_hash.
/// Format: {iterations}.{salt_base64}.{hash_base64}
/// </summary>
public sealed class CinturonPasswordHasher
{
    public const int DefaultIterations = 310_000;
    public const int SaltSize = 16;
    public const int HashSize = 32;

    public string Hash(string plaintext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(plaintext),
            salt,
            DefaultIterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return $"{DefaultIterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string plaintext, string storedHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plaintext);
        ArgumentException.ThrowIfNullOrWhiteSpace(storedHash);

        var parts = storedHash.Split('.');
        if (parts.Length != 3)
            return false;

        if (!int.TryParse(parts[0], out var iterations))
            return false;

        var salt = Convert.FromBase64String(parts[1]);
        var expectedHash = Convert.FromBase64String(parts[2]);
        var actualHash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(plaintext),
            salt,
            iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }
}
