using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;

namespace Cinturon360.Data.Repositories;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}
