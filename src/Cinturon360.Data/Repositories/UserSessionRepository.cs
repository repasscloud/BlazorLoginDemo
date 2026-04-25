using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Repositories;

public sealed class UserSessionRepository(AppDbContext db) : IUserSessionRepository
{
    public Task<UserSession?> GetByJtiAsync(string jti, CancellationToken ct = default)
        => db.UserSessions.FirstOrDefaultAsync(s => s.Jti == jti, ct);

    public async Task<IReadOnlyList<UserSession>> GetActiveByUserIdAsync(string userId, CancellationToken ct = default)
        => await db.UserSessions
            .Where(s => s.UserId == userId && !s.IsRevoked && s.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(ct);

    public async Task AddAsync(UserSession session, CancellationToken ct = default)
        => await db.UserSessions.AddAsync(session, ct);
}
