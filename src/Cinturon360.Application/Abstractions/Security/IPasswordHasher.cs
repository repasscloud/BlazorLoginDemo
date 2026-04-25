namespace Cinturon360.Application.Abstractions.Security;

/// <summary>
/// Password hashing service — implemented in Infrastructure.
/// Uses a strong KDF (BCrypt or Argon2id).
/// </summary>
public interface IPasswordHasher
{
    string Hash(string plaintext);
    bool Verify(string plaintext, string hash);
}
