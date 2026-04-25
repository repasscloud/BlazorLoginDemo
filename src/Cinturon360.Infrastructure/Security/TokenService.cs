using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Cinturon360.Application.Abstractions.Security;
using AppClaims = Cinturon360.Common.Constants.ClaimTypes;

namespace Cinturon360.Infrastructure.Security;

public sealed class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public string GenerateAccessToken(
        string userId,
        string? orgId,
        string jti,
        IReadOnlyList<string> permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId),
            new(JwtRegisteredClaimNames.Jti, jti),
            new(AppClaims.UserId, userId)
        };

        if (orgId is not null)
            claims.Add(new Claim(AppClaims.OrgId, orgId));

        foreach (var perm in permissions)
            claims.Add(new Claim("perm", perm));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool ValidateTokenHash(string rawToken, string storedHash)
        => HashToken(rawToken) == storedHash;

    public string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public (string RawToken, string Prefix, string Hash) GeneratePat(string prefix = "c360pat")
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = $"{prefix}_{Convert.ToBase64String(randomBytes).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
        var shortDisplay = rawToken[..Math.Min(rawToken.Length, prefix.Length + 9)];
        var hash = HashToken(rawToken);
        return (rawToken, shortDisplay, hash);
    }
}
