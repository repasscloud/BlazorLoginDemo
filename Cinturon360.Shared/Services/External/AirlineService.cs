using System.Globalization;
using System.Linq.Expressions;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Kernel.Travel;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Models.Static.Travel;
using Cinturon360.Shared.Services.Interfaces.External;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace Cinturon360.Shared.Services.External;

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

    // Service: no exceptions for validation/not-found; logs at each branch
    public async Task<UpdateAllianceResult> UpdateAirlineAllianceAsync(
        string? iata_icao, AirlineAlliance alliance, CancellationToken ct = default)
    {
        await _logger.InformationAsync(
            evt: "AIRLINE_ALLIANCE_UPDATE_START",
            cat: SysLogCatType.Data,
            act: SysLogActionType.Start,
            message: "Update airline alliance request",
            ent: nameof(Airline),
            entId: iata_icao ?? "",
            note: $"alliance:{alliance}");

        if (string.IsNullOrWhiteSpace(iata_icao) || (iata_icao.Length != 2 && iata_icao.Length != 3))
        {
            await _logger.WarningAsync(
                evt: "AIRLINE_ALLIANCE_UPDATE_BAD_CODE",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Validate,
                message: "IATA/ICAO code must be either 2 or 3 characters",
                ent: nameof(Airline),
                entId: iata_icao ?? "",
                note: "bad_request");
            return UpdateAllianceResult.BadRequest;
        }

        Airline? existing = null;
        var code = iata_icao.Trim().ToUpperInvariant();

        if (code.Length == 2)
            existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Iata == code, ct);
        else
            existing = await _db.Airlines.FirstOrDefaultAsync(a => a.Icao == code, ct);

        if (existing is null)
        {
            await _logger.WarningAsync(
                evt: "AIRLINE_ALLIANCE_UPDATE_NOT_FOUND",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                message: "Airline not found for provided code",
                ent: nameof(Airline),
                entId: code,
                note: "not_found");
            return UpdateAllianceResult.NotFound;
        }

        try
        {
            existing.Alliance = alliance;
            await _db.SaveChangesAsync(ct);

            await _logger.InformationAsync(
                evt: "AIRLINE_ALLIANCE_UPDATE_OK",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                message: $"Alliance updated to {alliance}",
                ent: nameof(Airline),
                entId: code);

            return UpdateAllianceResult.Ok;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(
                evt: "AIRLINE_ALLIANCE_UPDATE_ERR",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                ex: ex,
                message: "Failed to update airline alliance",
                ent: nameof(Airline),
                entId: code);
            return UpdateAllianceResult.BadRequest; // or map to a 500 at controller if you prefer
        }
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
        var failed = 0;

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
            HasHeaderRecord = false,
            Quote = '"',
            BadDataFound = null,
            MissingFieldFound = null,
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim
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
            var idRaw = Clean(csv.GetField(0));
            var name = Clean(csv.GetField(1));
            var alias = Clean(csv.GetField(2));
            var iata = CodeOrEmpty(csv.GetField(3), 2);
            var icao = CodeOrEmpty(csv.GetField(4), 3);
            var callsign = Clean(csv.GetField(5));
            var country = Clean(csv.GetField(6));
            var activeRaw = Clean(csv.GetField(7));
            var active = string.Equals(activeRaw, "Y", StringComparison.OrdinalIgnoreCase);

            _ = int.TryParse(idRaw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var idValue);

            if (idValue < 0) { skipped++; continue; }
            if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }
            if (string.IsNullOrEmpty(iata) && string.IsNullOrEmpty(icao)) { skipped++; continue; }

            if (name.Length > 200) name = name[..200];
            if (alias.Length > 200) alias = alias[..200];
            if (callsign.Length > 200) callsign = callsign[..200];
            if (country.Length > 100) country = country[..100];

            var candidate = new Airline
            {
                Iata = iata,
                Icao = icao,
                Name = name,
                Alias = alias,
                CallSign = callsign,
                Country = country,
                Alliance = AirlineAlliance.None,
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
                    existing.Name = candidate.Name;
                    existing.Alias = candidate.Alias;
                    existing.CallSign = candidate.CallSign;
                    existing.Country = candidate.Country;
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

        try
        {
            _ = await FixAirlineCodeAsync(ct);
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync(
                evt: "AIRLINE_IATA_INGEST_FINALIZE_ERROR",
                cat: SysLogCatType.Automation,
                act: SysLogActionType.End,
                ex: ex,
                message: "Finalization error during Airline IATA ingest",
                ent: "AirlineIataIngest",
                entId: runId,
                note: "cron:finalize_exception");
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

    // Domain result
    public enum UpdateAllianceResult { Ok, BadRequest, NotFound }

    private Task<int> FixAirlineCodeAsync(CancellationToken ct = default) =>
        _db.Airlines
        .Where(a => a.Icao == "SWR" && a.Iata != "LX")
        .ExecuteUpdateAsync(s => s.SetProperty(a => a.Iata, _ => "LX"), ct);
}