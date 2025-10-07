using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.Static;

namespace BlazorLoginDemo.Shared.Services.Interfaces.Kernel;

public interface IAirportInfoService
{
    // CREATE
    Task<AirportInfo> CreateAsync(AirportInfo airport, CancellationToken ct = default);

    // READ (single)
    Task<AirportInfo?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<AirportInfo?> GetByIdentAsync(string ident, CancellationToken ct = default);
    Task<AirportInfo?> GetByIataAsync(string iata, CancellationToken ct = default);
    Task<AirportInfo?> GetByGpsAsync(string gpsCode, CancellationToken ct = default);

    // READ (collections)
    Task<IReadOnlyList<AirportInfo>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<AirportInfo>> SearchMultiAsync(
        string? query = null,
        IReadOnlyList<AirportType>? types = null,
        IReadOnlyList<AirportContinent>? continents = null,
        IReadOnlyList<Iso3166_Alpha2>? countries = null,
        bool hasIata = true,          // flag (default true)
        bool hasMunicipality = true,  // flag (default true)
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    /// <summary>
    /// Flexible search: free-text over Name/Municipality/Ident/IATA
    /// + optional filters & paging.
    /// </summary>
    Task<IReadOnlyList<AirportInfo>> SearchAsync(
        string? query = null,
        AirportType? type = null,
        AirportContinent? continent = null,
        Iso3166_Alpha2? country = null,
        int skip = 0,
        int take = 50,
        CancellationToken ct = default);

    Task<IReadOnlyList<AirportInfo>> GetByCountry(Iso3166_Alpha2 isoCountry, CancellationToken ct = default);

    // UPDATE
    Task<AirportInfo> UpdateAsync(AirportInfo airport, CancellationToken ct = default);

    // DELETE
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // UTIL
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Upsert by Ident (case-insensitive). Returns number of rows affected (adds + updates).
    /// Intended for CSV/ETL pipelines.
    /// </summary>
    Task<int> BulkUpsertAsync(IEnumerable<AirportInfo> batch, CancellationToken ct = default);
}
