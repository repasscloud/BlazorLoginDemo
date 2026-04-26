using System.Security.Cryptography;
using System.Text;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Common.Security;

namespace Cinturon360.Infrastructure.Security;

/// <summary>
/// PBKDF2-based password hasher using SHA-512.
/// Format: {iterations}.{salt_base64}.{hash_base64}
/// </summary>
public sealed class PasswordHasher : IPasswordHasher
{
    private readonly CinturonPasswordHasher _inner = new();

    public string Hash(string plaintext)
        => _inner.Hash(plaintext);

    public bool Verify(string plaintext, string storedHash)
        => _inner.Verify(plaintext, storedHash);
}
