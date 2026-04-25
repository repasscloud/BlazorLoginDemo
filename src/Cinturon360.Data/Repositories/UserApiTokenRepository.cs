using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Repositories;

public sealed class UserApiTokenRepository(AppDbContext db) : IUserApiTokenRepository
{
    public Task<UserApiToken?> GetByTokenHashAsync(string tokenHash, CancellationToken ct = default)
        => db.UserApiTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task<IReadOnlyList<UserApiToken>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default)
        => await db.UserApiTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

    public async Task AddAsync(UserApiToken token, CancellationToken ct = default)
        => await db.UserApiTokens.AddAsync(token, ct);
}
