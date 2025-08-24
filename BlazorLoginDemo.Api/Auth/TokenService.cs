using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlazorLoginDemo.Shared.Models.Auth;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BlazorLoginDemo.Api.Auth;

public class TokenService
{
    private readonly JwtOptions _jwt;
    private readonly ApplicationDbContext _db;
    private readonly UserManager<AvaUser> _userManager;

    public TokenService(IOptions<JwtOptions> jwt, ApplicationDbContext db, UserManager<AvaUser> userManager)
    {
        _jwt = jwt.Value;
        _db = db;
        _userManager = userManager;
    }

    public async Task<(string accessToken, RefreshToken refresh)> GenerateTokensAsync(AvaUser user, string? ip = null, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        // Access token (JWT)
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, user.AspNetUsersId ?? user.Email ?? user.Id),
        };

        // Add roles if you use them
        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: creds);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        // Refresh token (opaque)
        var refresh = new RefreshToken
        {
            UserId = user.Id,
            Token = GenerateSecureToken(64),
            ExpiresUtc = now.AddDays(_jwt.RefreshTokenDays),
            CreatedUtc = now,
            CreatedByIp = ip
        };

        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync(ct);

        return (accessToken, refresh);
    }

    public async Task<(string accessToken, RefreshToken refresh)?> RefreshAsync(string refreshToken, string? ip = null, CancellationToken ct = default)
    {
        var existing = await _db.RefreshTokens.Include(r => r.User)
                        .FirstOrDefaultAsync(r => r.Token == refreshToken, ct);

        if (existing == null || existing.RevokedUtc != null || existing.ExpiresUtc <= DateTime.UtcNow)
            return null;

        // Rotate
        existing.RevokedUtc = DateTime.UtcNow;
        existing.RevokedByIp = ip;
        var (newAccess, newRefresh) = await GenerateTokensAsync(existing.User, ip, ct);
        existing.ReplacedByToken = newRefresh.Token;

        await _db.SaveChangesAsync(ct);
        return (newAccess, newRefresh);
    }

    public async Task<bool> RevokeAsync(string refreshToken, string? ip = null, string? reason = null, CancellationToken ct = default)
    {
        var rt = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken, ct);
        if (rt == null || rt.RevokedUtc != null) return false;

        rt.RevokedUtc = DateTime.UtcNow;
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
