using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Repositories;

public sealed class UserSecurityRepository(AppDbContext db) : IUserSecurityRepository
{
    public Task<UserSecurity?> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => db.UserSecurities.FirstOrDefaultAsync(s => s.UserId == userId, ct);

    public async Task AddAsync(UserSecurity security, CancellationToken ct = default)
        => await db.UserSecurities.AddAsync(security, ct);
}
