using System.Security.Cryptography;
using System.Text;
using Cinturon360.Application.Abstractions.Security;

namespace Cinturon360.Infrastructure.Security;

/// <summary>
/// PBKDF2-based password hasher using SHA-512.
/// Format: {iterations}.{salt_base64}.{hash_base64}
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private const int Iterations = 310_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string plaintext)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(plaintext),
            salt,
            Iterations,
            HashAlgorithmName.SHA512,
            HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string plaintext, string storedHash)
    {
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
