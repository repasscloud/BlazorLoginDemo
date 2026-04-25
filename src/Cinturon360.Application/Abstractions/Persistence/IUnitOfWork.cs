namespace Cinturon360.Application.Abstractions.Persistence;

/// <summary>
/// Unit of work — wraps a transaction across repositories.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
