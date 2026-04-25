using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByIdAsync(string id, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant().Trim(), ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await db.Users.AddAsync(user, ct);

    public async Task<IReadOnlyList<User>> GetByOrgIdAsync(string orgId, CancellationToken ct = default)
        => await db.Users.Where(u => u.HomeOrgId == orgId).ToListAsync(ct);
}
