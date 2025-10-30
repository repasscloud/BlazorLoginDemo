using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Cinturon360.Shared.Models.Auth;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Cinturon360.Api.Auth;

public class TokenService
{
    private readonly JwtOptions _jwt;
    private readonly ApplicationDbContext _db;

    public TokenService(IOptions<JwtOptions> jwt, ApplicationDbContext db)
    {
        _jwt = jwt.Value;
        _db = db;
    }

    public async Task<(string accessToken, RefreshToken refresh)> GenerateTokensAsync(
        AvaUser user, string? ip = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.AspNetUsersId ?? user.Email ?? user.Id),
        };

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refresh = new RefreshToken
        {
            AvaUserId   = user.Id,
            Token       = GenerateSecureToken(64),
            ExpiresUtc  = now.AddDays(_jwt.RefreshTokenDays),
            CreatedUtc  = now,
            CreatedByIp = ip
        };

        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync(ct);

        return (accessToken, refresh);
    }

    public async Task<(string accessToken, RefreshToken refresh)?> RefreshAsync(
        string refreshToken, string? ip = null, CancellationToken ct = default)
    {
        var existing = await _db.RefreshTokens
            .Include(r => r.AvaUser)
            .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

        if (existing == null || existing.RevokedUtc != null || existing.ExpiresUtc <= DateTime.UtcNow)
            return null;

        existing.RevokedUtc = DateTime.UtcNow;
        existing.RevokedByIp = ip;

        (string newAccess, RefreshToken newRefresh) =
            await GenerateTokensAsync(existing.AvaUser, ip, ct);

        existing.ReplacedByToken = newRefresh.Token;
        await _db.SaveChangesAsync(ct);

        return (newAccess, newRefresh);
    }

    public async Task<bool> RevokeAsync(
        string refreshToken, string? ip = null, string? reason = null, CancellationToken ct = default)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken, ct);
        if (rt == null || rt.RevokedUtc != null) return false;

        rt.RevokedUtc  = DateTime.UtcNow;
        rt.RevokedByIp = ip;
        rt.ReasonRevoked = reason ?? "User requested";
        await _db.SaveChangesAsync(ct);
        return true;
    }

    private static string GenerateSecureToken(int bytesLength)
    {
        var bytes = RandomNumberGenerator.GetBytes(bytesLength);
        return Convert.ToBase64String(bytes);
    }
}
