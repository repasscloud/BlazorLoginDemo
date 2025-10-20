using System.Globalization;
using System.Linq.Expressions;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.Static.SysVar;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace BlazorLoginDemo.Shared.Services.External;

public sealed class AirlineService : IAirlineService
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AirlineIngestionOptions _opts;
    private readonly ILoggerService _logger;

    public AirlineService(
        ApplicationDbContext db,
        IHttpClientFactory httpClientFactory,
        IOptions<AirlineIngestionOptions> opts,
        ILoggerService logger)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
        _opts = opts.Value;
        _logger = logger;
    }


    // ----- Reads (NoTracking) -----
    public async Task<Airline?> GetByIdAsync(int id, bool includeProgram = false, CancellationToken ct = default)
        => await BaseQuery(includeProgram)
              .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<Airline?> GetByIataAsync(string iata, bool includeProgram = false, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(iata)) return null;
        iata = iata.Trim().ToUpperInvariant();
        return await BaseQuery(includeProgram)
            .FirstOrDefaultAsync(a => a.Iata == iata, ct);
    }

    public async Task<Airline?> GetByIcaoAsync(string icao, bool includeProgram = false, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(icao)) return null;
        icao = icao.Trim().ToUpperInvariant();
        return await BaseQuery(includeProgram)
            .FirstOrDefaultAsync(a => a.Icao == icao, ct);
    }

    public async Task<List<Airline>> ListAsync(
        int skip = 0, int take = 200, string? country = null, CancellationToken ct = default)
    {
        IQueryable<Airline> q = BaseQuery(includeProgram: false);

        if (!string.IsNullOrWhiteSpace(country))
        {
            var c = country.Trim();
            q = q.Where(a => EF.Functions.ILike(a.Country, c));
        }

        return await q
            .OrderBy(a => a.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
    }

    public IAsyncEnumerable<Airline> StreamAllAsync(CancellationToken ct = default)
        => BaseQuery(includeProgram: false)
            .OrderBy(a => a.Name)
            .AsAsyncEnumerable();

    private IQueryable<Airline> BaseQuery(bool includeProgram)
    {
        var q = _db.Airlines.AsNoTracking();
        if (includeProgram) q = q.Include(a => a.LoyaltyProgram);
        return q;
    }


    // ----- Writes -----
    public async Task<int> AddAsync(Airline entity, CancellationToken ct = default)
    {
        _db.Airlines.Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(Airline entity, CancellationToken ct = default)
    {
        // Attach and mark modified for simple updates
        _db.Airlines.Attach(entity);
        _db.Entry(entity).State = EntityState.Modified;
        var changed = await _db.SaveChangesAsync(ct);
        return changed > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Id == id, ct);
        if (existing is null) return false;
        _db.Airlines.Remove(existing);
        return await _db.SaveChangesAsync(ct) > 0;
    }

    public async Task<int> UpsertByCodesAsync(Airline candidate, CancellationToken ct = default)
    {
        // Normalize keys
        var iata = string.IsNullOrWhiteSpace(candidate.Iata) ? null : candidate.Iata.Trim().ToUpperInvariant();
        var icao = string.IsNullOrWhiteSpace(candidate.Icao) ? null : candidate.Icao.Trim().ToUpperInvariant();

        Airline? existing = null;

        if (!string.IsNullOrEmpty(iata))
        {
            existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Iata == iata, ct);
        }
        if (existing is null && !string.IsNullOrEmpty(icao))
        {
            existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Icao == icao, ct);
        }

        if (existing is null)
        {
            // Insert
            candidate.Iata = iata ?? string.Empty;
            candidate.Icao = icao ?? string.Empty;
            _db.Airlines.Add(candidate);
            await _db.SaveChangesAsync(ct);
            return candidate.Id;
        }

        // Update selected fields only
        existing.Name        = candidate.Name;
        existing.Alias       = candidate.Alias;
        existing.CallSign    = candidate.CallSign;
        existing.Country     = candidate.Country;
        existing.Alliance    = candidate.Alliance;
        existing.FoundedYear = candidate.FoundedYear;
        existing.Iata        = iata ?? existing.Iata;
        existing.Icao        = icao ?? existing.Icao;

        await _db.SaveChangesAsync(ct);
        return existing.Id;
    }

    public Task<bool> ExistsAsync(Expression<Func<Airline, bool>> predicate, CancellationToken ct = default)
        => _db.Airlines.AsNoTracking().AnyAsync(predicate, ct);


    // ---------- Ingestion from configured source ----------

    // public async Task<AirlineImportResult> ImportFromConfiguredSourceAsync(CancellationToken ct = default)
    // {
    //     if (string.IsNullOrWhiteSpace(_opts.SourceUrl))
    //     {
    //         var ex = new InvalidOperationException("AirlineIngestion: SourceUrl is not configured.");
    //         await _logger.ErrorAsync(
    //             evt: "AIRLINE_INGEST_SOURCE_URL_MISSING",
    //             cat: SysLogCatType.Automation,          // cron/ingest workflow
    //             act: SysLogActionType.Validate,         // config validation failed
    //             ex: ex,
    //             message: "Airline IATA ingest configuration missing: SourceUrl",
    //             ent: "AirlineIataIngest",
    //             entId: "config",
    //             note: "config_missing");
    //         throw ex;
    //     }

    //     // START — Airline IATA ingest (cron)
    //     Guid runId = Guid.NewGuid();
    //     await _logger.InformationAsync(
    //         evt: "AIRLINE_IATA_INGEST_START",
    //         cat: SysLogCatType.Automation,
    //         act: SysLogActionType.Start,
    //         message: $"Airline IATA ingest starting (source={_opts.SourceUrl})",
    //         ent: "AirlineIataIngest",
    //         entId: $"{runId}",            // e.g., Guid or yyyyMMddHHmm
    //         note: "cron:start");

    //     var client = _httpClientFactory.CreateClient(_opts.HttpClientName);

    //     using var resp = await client.GetAsync(_opts.SourceUrl, HttpCompletionOption.ResponseHeadersRead, ct);
    //     resp.EnsureSuccessStatusCode();

    //     var created = 0;
    //     var updated = 0;

    //     await using var stream = await resp.Content.ReadAsStreamAsync(ct);
    //     using var reader = new StreamReader(stream);
    //     using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
    //     {
    //         HasHeaderRecord = false,
    //         Quote = '"',
    //         BadDataFound = null,
    //         MissingFieldFound = null
    //     });

    //     // airlines.dat columns:
    //     // 0 AirlineID, 1 Name, 2 Alias, 3 IATA, 4 ICAO, 5 Callsign, 6 Country, 7 Active(Y/N)
    //     while (await csv.ReadAsync())
    //     {
    //         ct.ThrowIfCancellationRequested();

    //         var name = (csv.GetField(1) ?? "").Trim();
    //         var alias = (csv.GetField(2) ?? "").Trim();
    //         var iata = (csv.GetField(3) ?? "").Trim().ToUpperInvariant();
    //         var icao = (csv.GetField(4) ?? "").Trim().ToUpperInvariant();
    //         var callsign = (csv.GetField(5) ?? "").Trim();
    //         var country = (csv.GetField(6) ?? "").Trim();
    //         var active = (csv.GetField(7) ?? "Y").Trim().Equals("Y", StringComparison.OrdinalIgnoreCase);

    //         // Skip obviously bad rows
    //         if (string.IsNullOrWhiteSpace(name)) continue;
    //         if (string.IsNullOrWhiteSpace(iata) && string.IsNullOrWhiteSpace(icao)) continue;

    //         // Build candidate
    //         var candidate = new Airline
    //         {
    //             Iata = iata,
    //             Icao = icao,
    //             Name = name,
    //             Alias = alias,
    //             CallSign = callsign,
    //             Country = country,
    //             Alliance = Models.Static.Travel.AirlineAlliance.Unknown,
    //             FoundedYear = null
    //         };

    //         // Upsert and count
    //         var idBefore = 0;
    //         Airline? existing = null;

    //         if (!string.IsNullOrEmpty(iata))
    //             existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Iata == iata, ct);
    //         if (existing is null && !string.IsNullOrEmpty(icao))
    //             existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Icao == icao, ct);

    //         if (existing is null)
    //         {
    //             _db.Airlines.Add(candidate);
    //             await _db.SaveChangesAsync(ct);
    //             created++;
    //         }
    //         else
    //         {
    //             idBefore = existing.Id;
    //             existing.Name = candidate.Name;
    //             existing.Alias = candidate.Alias;
    //             existing.CallSign = candidate.CallSign;
    //             existing.Country = candidate.Country;
    //             // keep Alliance/FoundedYear as-is unless you want to reset:
    //             // existing.Alliance    = candidate.Alliance;
    //             // existing.FoundedYear = candidate.FoundedYear;
    //             if (!string.IsNullOrEmpty(iata)) existing.Iata = iata;
    //             if (!string.IsNullOrEmpty(icao)) existing.Icao = icao;

    //             await _db.SaveChangesAsync(ct);
    //             updated++;
    //         }
    //     }

    //     // FINISH — include created/updated totals
    //     await _logger.InformationAsync(
    //         evt: "AIRLINE_IATA_INGEST_FINISH",
    //         cat: SysLogCatType.Automation,
    //         act: SysLogActionType.End,
    //         message: $"Airline IATA ingest finished. created={created} updated={updated}",
    //         ent: "AirlineIataIngest",
    //         entId: $"{runId}",
    //         note: "cron:finish");

    //     return new AirlineImportResult(created, updated);
    // }
    public async Task<AirlineImportResult> ImportFromConfiguredSourceAsync(CancellationToken ct = default)
    {
        // Config check
        if (string.IsNullOrWhiteSpace(_opts.SourceUrl))
        {
            var ex = new InvalidOperationException("AirlineIngestion: SourceUrl is not configured.");
            await _logger.ErrorAsync(
                evt: "AIRLINE_INGEST_SOURCE_URL_MISSING",
                cat: SysLogCatType.Automation,
                act: SysLogActionType.Validate,
                ex: ex,
                message: "Airline IATA ingest configuration missing: SourceUrl",
                ent: "AirlineIataIngest",
                entId: "config",
                note: "config_missing");
            throw ex;
        }

        // START
        var runId = Guid.NewGuid().ToString("N");
        await _logger.InformationAsync(
            evt: "AIRLINE_IATA_INGEST_START",
            cat: SysLogCatType.Automation,
            act: SysLogActionType.Start,
            message: $"Airline IATA ingest starting (source={_opts.SourceUrl})",
            ent: "AirlineIataIngest",
            entId: runId,
            note: "cron:start");

        var created = 0;
        var updated = 0;
        var skipped = 0;
        var failed  = 0;

        // HTTP fetch with error logging
        HttpResponseMessage resp;
        try
        {
            var client = _httpClientFactory.CreateClient(_opts.HttpClientName);
            resp = await client.GetAsync(_opts.SourceUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync(ct);
                var ex = new HttpRequestException($"HTTP {(int)resp.StatusCode} {resp.ReasonPhrase}");
                await _logger.ErrorAsync(
                    evt: "AIRLINE_IATA_INGEST_HTTP_ERROR",
                    cat: SysLogCatType.Integration,
                    act: SysLogActionType.Exec,
                    ex: ex,
                    message: "Airline IATA ingest HTTP error",
                    ent: "AirlineIataIngest",
                    entId: runId,
                    stat: (int)resp.StatusCode,
                    note: "provider:OpenFlights/raw.githubusercontent.com");
                throw ex;
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(
                evt: "AIRLINE_IATA_INGEST_HTTP_EXCEPTION",
                cat: SysLogCatType.Integration,
                act: SysLogActionType.Exec,
                ex: ex,
                message: "Exception during Airline IATA ingest HTTP fetch",
                ent: "AirlineIataIngest",
                entId: runId,
                note: "provider:OpenFlights/raw.githubusercontent.com");
            throw;
        }

        await using var stream = await resp.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord   = false,
            Quote             = '"',
            BadDataFound      = null,
            MissingFieldFound = null,
            TrimOptions       = CsvHelper.Configuration.TrimOptions.Trim
        });

        static string Clean(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var t = s.Trim();
            return t.Equals(@"\N", StringComparison.OrdinalIgnoreCase) ? string.Empty : t;
        }

        static string CodeOrEmpty(string? s, int len)
        {
            var t = Clean(s).ToUpperInvariant();
            if (t.Length != len) return string.Empty;
            for (int i = 0; i < t.Length; i++) if (!char.IsLetterOrDigit(t[i])) return string.Empty;
            return t;
        }

        while (await csv.ReadAsync())
        {
            ct.ThrowIfCancellationRequested();

            // 1: Id, 2: Name, 3: Alias, 4: IATA, 5: ICAO, 6: Callsign, 7: Country, 8: Active(Y/N)
            var idRaw     = Clean(csv.GetField(0));
            var name      = Clean(csv.GetField(1));
            var alias     = Clean(csv.GetField(2));
            var iata      = CodeOrEmpty(csv.GetField(3), 2);
            var icao      = CodeOrEmpty(csv.GetField(4), 3);
            var callsign  = Clean(csv.GetField(5));
            var country   = Clean(csv.GetField(6));
            var activeRaw = Clean(csv.GetField(7));
            var active    = string.Equals(activeRaw, "Y", StringComparison.OrdinalIgnoreCase);

            _ = int.TryParse(idRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idValue);

            if (idValue < 0) { skipped++; continue; }
            if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }
            if (string.IsNullOrEmpty(iata) && string.IsNullOrEmpty(icao)) { skipped++; continue; }

            if (name.Length     > 200) name     = name[..200];
            if (alias.Length    > 200) alias    = alias[..200];
            if (callsign.Length > 200) callsign = callsign[..200];
            if (country.Length  > 100) country  = country[..100];

            var candidate = new Airline
            {
                Iata        = iata,
                Icao        = icao,
                Name        = name,
                Alias       = alias,
                CallSign    = callsign,
                Country     = country,
                Alliance    = Models.Static.Travel.AirlineAlliance.Unknown,
                FoundedYear = null
            };

            try
            {
                Airline? existing = null;

                if (!string.IsNullOrEmpty(iata))
                    existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Iata == iata, ct);

                if (existing is null && !string.IsNullOrEmpty(icao))
                    existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Icao == icao, ct);

                if (existing is null)
                {
                    _db.Airlines.Add(candidate);
                    await _db.SaveChangesAsync(ct);
                    created++;
                }
                else
                {
                    existing.Name     = candidate.Name;
                    existing.Alias    = candidate.Alias;
                    existing.CallSign = candidate.CallSign;
                    existing.Country  = candidate.Country;
                    if (!string.IsNullOrEmpty(iata)) existing.Iata = iata;
                    if (!string.IsNullOrEmpty(icao)) existing.Icao = icao;

                    await _db.SaveChangesAsync(ct);
                    updated++;
                }
            }
            catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
            {
                failed++;
                await _logger.ErrorAsync(
                    evt: "AIRLINE_IATA_INGEST_DB_ERROR",
                    cat: SysLogCatType.Automation,
                    act: SysLogActionType.Update,
                    ex: ex,
                    message: $"DB error on airline upsert (iata={iata}, icao={icao}, name={name})",
                    ent: nameof(Airline),
                    entId: string.IsNullOrEmpty(iata) ? icao : iata,
                    note: $"pg:{pg.SqlState}|{pg.ConstraintName}|{pg.TableName}|{pg.ColumnName}");
                continue;
            }
            catch (Exception ex)
            {
                failed++;
                await _logger.ErrorAsync(
                    evt: "AIRLINE_IATA_INGEST_ROW_ERROR",
                    cat: SysLogCatType.Automation,
                    act: SysLogActionType.Update,
                    ex: ex,
                    message: $"Row ingest error (iata={iata}, icao={icao}, name={name})",
                    ent: nameof(Airline),
                    entId: string.IsNullOrEmpty(iata) ? icao : iata,
                    note: "row_exception");
                continue;
            }
        }

        // FINISH
        await _logger.InformationAsync(
            evt: "AIRLINE_IATA_INGEST_FINISH",
            cat: SysLogCatType.Automation,
            act: SysLogActionType.End,
            message: $"Airline IATA ingest finished. created={created} updated={updated} skipped={skipped} failed={failed}",
            ent: "AirlineIataIngest",
            entId: runId,
            note: "cron:finish");

        return new AirlineImportResult(created, updated);
    }
}