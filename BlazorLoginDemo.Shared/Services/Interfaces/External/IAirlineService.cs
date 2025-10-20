using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using System.Linq.Expressions;

namespace BlazorLoginDemo.Shared.Services.Interfaces.External;

public interface IAirlineService
{
    // Reads (all NoTracking)
    Task<Airline?> GetByIdAsync(int id, bool includeProgram = false, CancellationToken ct = default);
    Task<Airline?> GetByIataAsync(string iata, bool includeProgram = false, CancellationToken ct = default);
    Task<Airline?> GetByIcaoAsync(string icao, bool includeProgram = false, CancellationToken ct = default);
    Task<List<Airline>> ListAsync(int skip = 0, int take = 200, string? country = null, CancellationToken ct = default);
    IAsyncEnumerable<Airline> StreamAllAsync(CancellationToken ct = default);

    // Writes
    Task<int> AddAsync(Airline entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(Airline entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // Upsert by natural keys. If IATA is present, it wins; else fall back to ICAO.
    Task<int> UpsertByCodesAsync(Airline candidate, CancellationToken ct = default);

    // Utility
    Task<bool> ExistsAsync(Expression<Func<Airline, bool>> predicate, CancellationToken ct = default);

    // Ingestion
    Task<AirlineImportResult> ImportFromConfiguredSourceAsync(CancellationToken ct = default);
}

public sealed record AirlineImportResult(int Created, int Updated);