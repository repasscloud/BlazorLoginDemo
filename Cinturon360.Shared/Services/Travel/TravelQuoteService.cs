// Services/Travel/TravelQuoteService.cs
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.DTOs;
using Cinturon360.Shared.Models.ExternalLib.Amadeus;
using Cinturon360.Shared.Models.ExternalLib.Amadeus.Flight;
using Cinturon360.Shared.Models.Kernel.Travel;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Models.Search;
using Cinturon360.Shared.Models.Static;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Models.Static.Travel;
using Cinturon360.Shared.Services.Interfaces.External;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Services.Interfaces.Platform;
using Cinturon360.Shared.Services.Interfaces.Policy;
using Cinturon360.Shared.Services.Interfaces.Travel;
using Microsoft.EntityFrameworkCore;
using System.Xml;
using Cinturon360.Shared.Models.Static.Billing;
using System.Net;

namespace Cinturon360.Shared.Services.Travel;

internal sealed class TravelQuoteService : ITravelQuoteService
{
    private readonly ApplicationDbContext _db;
    private readonly IAdminOrgServiceUnified _orgSvc;
    private readonly IAdminUserServiceUnified _userSvc;
    private readonly ITravelPolicyService _travelPolicySvc;
    private readonly IAirportInfoService _airportInfoSvc;
    private readonly IAirlineService _airlineSvc;
    private readonly IAdminOrgServiceUnified _orgService;
    private readonly ILoggerService _log;
    private readonly JsonSerializerOptions _jsonOptions;

    public TravelQuoteService(
        ApplicationDbContext db,
        IAdminOrgServiceUnified orgSvc,
        IAdminUserServiceUnified userSvc,
        ITravelPolicyService travelPolicySvc,
        IAirportInfoService airportInfoSvc,
        IAirlineService airlineSvc,
        IAdminOrgServiceUnified orgService,
        ILoggerService log,
        JsonSerializerOptions jsonOptions)
    {
        _db = db;
        _orgSvc = orgSvc;
        _userSvc = userSvc;
        _travelPolicySvc = travelPolicySvc;
        _airportInfoSvc = airportInfoSvc;
        _airlineSvc = airlineSvc;
        _orgService = orgService;
        _log = log;
        _jsonOptions = jsonOptions;
    }

    // ---------------- CREATE ----------------
    public async Task<TravelQuote> CreateAsync(TravelQuote model, CancellationToken ct = default)
    {
        await ValidateRootsAsync(model.OrganizationId, model.TmcAssignedId, model.CreatedByUserId, ct);
        await EnsureTravellerUsersExistAsync(model.Travellers.Select(t => t.UserId), ct);

        _db.TravelQuotes.Add(model);
        await _db.SaveChangesAsync(ct);
        return await LoadAggregateTrackedAsync(model.Id, ct); // return fresh tracked copy
    }

    public async Task<(bool Ok, string? Error, string? TravelQuoteId)> CreateFromDtoAsync(TravelQuoteDto dto, CancellationToken ct = default)
    {
        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_TRANSLATE_START",
            cat: SysLogCatType.Workflow,
            act: SysLogActionType.Start,
            message: $"Translate TravelQuote DTO start (createdby={dto.CreatedByUserId}, type={dto.QuoteType}, org={dto.OrganizationId}, travellers={dto.TravellerUserIds?.Count ?? 0})",
            ent: "TravelQuoteDto",
            entId: dto.OrganizationId,
            uid: dto.CreatedByUserId,
            org: dto.OrganizationId);

        try
        {
            var quote = await TranslateDtoAsync(dto, ct);

            if (quote.Travellers.Count == 0)
            {
                await _log.WarningAsync(
                    evt: "TRAVEL_QUOTE_TRANSLATE_NO_TRAVELLERS",
                    cat: SysLogCatType.Workflow,
                    act: SysLogActionType.Validate,
                    message: "No valid travellers with Travel Policy assigned to generate a quote. Assign users a Travel Policy or set an Org Default Travel Policy.",
                    ent: "TravelQuoteDto",
                    entId: dto.OrganizationId,
                    uid: dto.CreatedByUserId,
                    org: dto.OrganizationId,
                    note: "no_travellers_or_policy");

                await _log.InformationAsync(
                    evt: "TRAVEL_QUOTE_TRANSLATE_FINISH",
                    cat: SysLogCatType.Workflow,
                    act: SysLogActionType.End,
                    message: $"Translate TravelQuote DTO finished (type={dto.QuoteType}, org={dto.OrganizationId})",
                    ent: "TravelQuoteDto",
                    entId: dto.OrganizationId,
                    uid: dto.CreatedByUserId,
                    org: dto.OrganizationId);

                return (false, "No valid travellers with Travel Policy assigned to generate a quote. Assign users a Travel Policy or assign the organization a Default Travel Policy to generate a quote.", null);
            }

            _db.TravelQuotes.Add(quote);
            await _db.SaveChangesAsync(ct);
            return (true, null, quote.Id);
        }
        catch (Exception ex)
        {
            await _log.ErrorAsync(
                evt: "TRAVEL_QUOTE_CREATE_FROM_DTO_FAIL",
                cat: SysLogCatType.Workflow,
                act: SysLogActionType.Exec,
                ex: ex,
                message: "CreateFromDtoAsync failed",
                ent: "TravelQuoteDto",
                entId: dto.OrganizationId,
                uid: dto.CreatedByUserId,
                org: dto.OrganizationId);

            return (false, ex.GetBaseException().Message, null);
        }
    }

    // ---------------- READ (NO TRACKING) ----------------
    public async Task<TravelQuote?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        return await _db.TravelQuotes
            .AsNoTracking()
            .AsSplitQuery()
            .Include(q => q.Organization)
            .Include(q => q.TmcAssigned)
            .Include(q => q.CreatedBy)
            .Include(q => q.Travellers).ThenInclude(t => t.User)
            .FirstOrDefaultAsync(q => q.Id == id, ct);
    }

    public async Task<IReadOnlyList<TravelQuote>> SearchAsync(
        string? organizationId = null,
        string? createdByUserId = null,
        string? tmcAssignedId = null,
        TravelQuoteType? type = null,
        QuoteState? state = null,
        CancellationToken ct = default)
    {
        IQueryable<TravelQuote> q = _db.TravelQuotes
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Organization)
            .Include(x => x.TmcAssigned)
            .Include(x => x.CreatedBy)
            .Include(x => x.Travellers);

        if (!string.IsNullOrWhiteSpace(organizationId)) q = q.Where(x => x.OrganizationId == organizationId);
        if (!string.IsNullOrWhiteSpace(createdByUserId)) q = q.Where(x => x.CreatedByUserId == createdByUserId);
        if (!string.IsNullOrWhiteSpace(tmcAssignedId)) q = q.Where(x => x.TmcAssignedId == tmcAssignedId);
        if (type.HasValue) q = q.Where(x => x.Type == type.Value);
        if (state.HasValue) q = q.Where(x => x.State == state.Value);

        var list = await q.OrderBy(x => x.CreatedAtUtc).ThenBy(x => x.Id).ToListAsync(ct);
        return list;
    }

    // ---------------- UPDATE (PUT) ----------------
    public async Task<TravelQuote> UpdatePutAsync(TravelQuote incoming, CancellationToken ct = default)
    {
        // Full overwrite semantics
        _db.ChangeTracker.Clear();

        // Validate roots before touching DB (this is hard core error handling!)
        await ValidateRootsAsync(incoming.OrganizationId, incoming.TmcAssignedId, incoming.CreatedByUserId, ct);
        await EnsureTravellerUsersExistAsync(incoming.Travellers.Select(t => t.UserId), ct);

        var existing = await _db.TravelQuotes
            .Include(q => q.Travellers)
            .FirstOrDefaultAsync(q => q.Id == incoming.Id, ct)
            ?? throw new InvalidOperationException($"TravelQuote '{incoming.Id}' not found.");

        // Overwrite scalars
        existing.Type = incoming.Type;
        existing.State = incoming.State;
        existing.OrganizationId = incoming.OrganizationId;
        existing.TmcAssignedId = incoming.TmcAssignedId;
        existing.CreatedByUserId = incoming.CreatedByUserId;

        // Replace travellers collection atomically
        if (existing.Travellers.Count > 0)
            _db.RemoveRange(existing.Travellers);
        existing.Travellers.Clear();

        foreach (var t in incoming.Travellers)
        {
            existing.Travellers.Add(new TravelQuoteUser { TravelQuoteId = existing.Id, UserId = t.UserId });
        }

        await _db.SaveChangesAsync(ct);
        return await LoadAggregateTrackedAsync(existing.Id, ct);
    }

    // ---------------- POINT UPDATERS ----------------
    public async Task<bool> ReassignCreatedByAsync(string travelQuoteId, string newUserId, CancellationToken ct = default)
    {
        // validate new user exists
        if (!await _userSvc.ExistsAsync(newUserId, ct))
            throw new InvalidOperationException($"User '{newUserId}' not found.");

        var q = await _db.TravelQuotes.FirstOrDefaultAsync(x => x.Id == travelQuoteId, ct)
            ?? throw new InvalidOperationException($"TravelQuote '{travelQuoteId}' not found.");

        q.CreatedByUserId = newUserId;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> UpdateStateAsync(string travelQuoteId, QuoteState newState, CancellationToken ct = default)
    {
        var q = await _db.TravelQuotes.FirstOrDefaultAsync(x => x.Id == travelQuoteId, ct)
            ?? throw new InvalidOperationException($"TravelQuote '{travelQuoteId}' not found.");

        q.State = newState;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task IngestTravelQuoteFlightUIResultPatchDto(TravelQuoteFlightUIResultPatchDto dto, CancellationToken ct = default)
    {
        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_UI_PATCH_START",
            cat: SysLogCatType.Data,
            act: SysLogActionType.Start,
            message: "Ingesting TravelQuote flight UI patch",
            ent: nameof(TravelQuote),
            entId: dto.Id,
            note: "ui_patch");

        var q = await _db.TravelQuotes.FirstOrDefaultAsync(x => x.Id == dto.Id, ct);
        if (q is null)
        {
            var ex = new InvalidOperationException($"TravelQuote '{dto.Id}' not found.");
            await _log.ErrorAsync(
                evt: "TRAVEL_QUOTE_UPDATE_NOT_FOUND",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                ex: ex,
                message: $"TravelQuote '{dto.Id}' not found.",
                ent: nameof(TravelQuote),
                entId: dto.Id,
                note: "not_found");
            throw ex;
        }

        // Update fields if present in DTO
        q.TripType = dto.TripType ?? null;
        q.OriginIataCode = dto.OriginIataCode ?? null;
        q.DestinationIataCode = dto.DestinationIataCode ?? null;
        q.DepartureDate = dto.DepartureDate ?? null;
        q.ReturnDate = dto.ReturnDate ?? null;
        q.DepartEarliestTime = dto.DepartEarliestTime ?? null;
        q.DepartLatestTime = dto.DepartLatestTime ?? null;
        q.ReturnEarliestTime = dto.ReturnEarliestTime ?? null;
        q.ReturnLatestTime = dto.ReturnLatestTime ?? null;
        q.CabinClass = dto.CabinClass ?? null;
        q.MaxCabinClass = dto.MaxCabinClass ?? null;
        q.SelectedAirlines = dto.SelectedAirlines.Length > 0 ? dto.SelectedAirlines : Array.Empty<string>();
        q.Alliances = dto.Alliances ?? null;
        q.State = QuoteState.SearchInProgress;  // mark as having UI data ingested
        q.UpdatedAtUtc = DateTime.UtcNow;

        q.Note = (q.Note ?? "") +
            $"\n[Auto-Updated {DateTime.UtcNow:o} UTC] Flight Search UI results ingested.";

        await _db.SaveChangesAsync(ct);

        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_UPDATED",
            cat: SysLogCatType.Data,
            act: SysLogActionType.Update,
            message: "TravelQuote updated from flight UI patch",
            ent: nameof(TravelQuote),
            entId: dto.Id,
            note: "ui_patch");

        return;
    }

    // ---------------- DELETE ----------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var q = await _db.TravelQuotes.Include(x => x.Travellers).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (q is null) return false;

        if (q.Travellers.Count > 0) _db.RemoveRange(q.Travellers);
        _db.TravelQuotes.Remove(q);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // ---------------- HELPERS ----------------
    public bool TryParseQuoteType(string value, out TravelQuoteType type)
    {
        type = TravelQuoteType.Unknown;
        if (string.IsNullOrWhiteSpace(value)) return false;

        // Normalize common synonyms if needed
        var v = value.Trim();
        // Enum names are lower-case in this model; allow case-insensitive parse.
        if (Enum.TryParse<TravelQuoteType>(v, ignoreCase: true, out var parsed))
        {
            type = parsed;
            return true;
        }
        return false;
    }

    public async Task<int> ExpireOldQuotesAsync(CancellationToken ct = default)
    {
        var cutoffUtc = DateTime.UtcNow.AddDays(-3);

        var affected = await _db.TravelQuotes
            .Where(q => q.CreatedAtUtc < cutoffUtc && q.State != QuoteState.Expired)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(q => q.State, q => QuoteState.Expired), ct);

        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_EXPIRE_OLD",
            cat: SysLogCatType.Automation,          // background maintenance
            act: SysLogActionType.Update,           // bulk state change
            message: $"Expired {affected} travel quotes older than {cutoffUtc:o}",
            ent: nameof(TravelQuote),
            entId: $"cutoff:{cutoffUtc:o}",
            note: "bulk_expire");

        return affected;
    }

    public async Task<List<string>?> GetExcludedAirlinesFromPolicyAsync(string travelPolicyId, TravelQuotePolicyType policyType, CancellationToken ct = default)
    {
        // Implementation goes here
        switch (policyType)
        {
            case TravelQuotePolicyType.OrgDefault:
            case TravelQuotePolicyType.Unknown:
                return await _db.TravelPolicies.Where(p => p.Id == travelPolicyId).FirstOrDefaultAsync(ct) is TravelPolicy tp
                    ? tp.ExcludedAirlineCodesCsv?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                    : null;
            case TravelQuotePolicyType.Ephemeral:
                return await _db.EphemeralTravelPolicies.Where(p => p.Id == travelPolicyId).FirstOrDefaultAsync(ct) is EphemeralTravelPolicy etp
                   ? etp.ExcludedAirlineCodesCsv?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                   : null;
            case TravelQuotePolicyType.UserDefined:
                return null;
            default:
                return null;
        }
    }

    // ---------------- UI HELPERS ----------------
    // this can only be called via the API layer where the process is able to consume resources
    public async Task<FlightSearchPageConfig> GenerateFlightSearchUIOptionsAsync(string travelQuoteId, CancellationToken ct = default)
    {
        var quote = await GetByIdAsync(travelQuoteId, ct)
            ?? throw new InvalidOperationException($"TravelQuote '{travelQuoteId}' not found.");

        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_START",
            cat: SysLogCatType.App,
            act: SysLogActionType.Start,
            message: $"Generating flight search UI options for TravelQuote '{travelQuoteId}'",
            ent: nameof(TravelQuote),
            entId: travelQuoteId);

        var orgName = quote.Organization?.Name ?? "Unknown Org";

        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS",
            cat: SysLogCatType.App,
            act: SysLogActionType.Step,
            message: $"orgName set to '{orgName}'",
            ent: nameof(TravelQuote),
            entId: travelQuoteId);

        // Find policy
        TravelPolicy? travelPolicy = null;
        switch (quote.PolicyType)
        {
            case TravelQuotePolicyType.OrgDefault:
            case TravelQuotePolicyType.UserDefined:
                await _log.InformationAsync(
                    evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS",
                    cat: SysLogCatType.App,
                    act: SysLogActionType.Step,
                    message: $"TravelPolicy type is '{quote.PolicyType}' for TravelQuote '{travelQuoteId}'",
                    ent: nameof(TravelQuote),
                    entId: travelQuoteId);
                travelPolicy = await _db.TravelPolicies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == quote.TravelPolicyId, ct);
                break;

            case TravelQuotePolicyType.Ephemeral:
                await _log.InformationAsync(
                    evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS",
                    cat: SysLogCatType.App,
                    act: SysLogActionType.Step,
                    message: $"TravelPolicy type is '{quote.PolicyType}' for TravelQuote '{travelQuoteId}'",
                    ent: nameof(TravelQuote),
                    entId: travelQuoteId);
                var eph = await _db.EphemeralTravelPolicies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == quote.TravelPolicyId, ct);

                if (eph is not null)
                {
                    travelPolicy = ConvertEphemeralToTravelPolicy(eph);
                    await _log.InformationAsync(
                        evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_EPHEMERAL_CONVERTED",
                        cat: SysLogCatType.Workflow,
                        act: SysLogActionType.Step,
                        message: $"Converted EphemeralTravelPolicy '{eph.Id}' from TravelPolicy for TravelQuote '{travelQuoteId}'",
                        ent: nameof(TravelQuote),
                        entId: travelQuoteId);
                }
                break;

            default:
                await _log.WarningAsync(
                    evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_UNKNOWN_POLICY_TYPE",
                    cat: SysLogCatType.Workflow,
                    act: SysLogActionType.Validate,
                    message: $"Unknown TravelPolicy type '{quote.PolicyType}' for TravelQuote '{travelQuoteId}'",
                    ent: nameof(TravelQuote),
                    entId: travelQuoteId,
                    note: "unknown_policy_type");
                break;
        }

        var policyName = travelPolicy?.PolicyName ?? "Unknown Policy";
        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS",
            cat: SysLogCatType.App,
            act: SysLogActionType.Step,
            message: $"TravelPolicy name '{policyName}' for TravelQuote '{travelQuoteId}'",
            ent: nameof(TravelQuote),
            entId: travelQuoteId);

        var adults = quote.Travellers.Count;
        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS",
            cat: SysLogCatType.App,
            act: SysLogActionType.Step,
            message: $"TravelPolicy adult count '{adults}' for TravelQuote '{travelQuoteId}'",
            ent: nameof(TravelQuote),
            entId: travelQuoteId);

        // Min departure date per policy
        DateTime earliestDepartureDate = DateTime.UtcNow;
        if (travelPolicy?.DefaultCalendarDaysInAdvanceForFlightBooking is int d && d > 0)
            earliestDepartureDate = DateTime.UtcNow.AddDays(d);

        // Fixed time window
        string? earliestDepartureTime = null;
        string? latestDepartureTime = null;
        bool hasFixedTimes = false;

        if (!string.IsNullOrWhiteSpace(travelPolicy?.FlightBookingTimeAvailableFrom) &&
            DateTime.TryParseExact(travelPolicy.FlightBookingTimeAvailableFrom, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t1))
        {
            earliestDepartureTime = t1.ToString("HH:mm", CultureInfo.InvariantCulture);
            hasFixedTimes = true;
        }
        if (!string.IsNullOrWhiteSpace(travelPolicy?.FlightBookingTimeAvailableTo) &&
            DateTime.TryParseExact(travelPolicy.FlightBookingTimeAvailableTo, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t2))
        {
            latestDepartureTime = t2.ToString("HH:mm", CultureInfo.InvariantCulture);
            hasFixedTimes = true;
        }

        // Allowed countries → airport list
        var allowedCountries = travelPolicy is not null
            ? await _travelPolicySvc.ResolveAllowedCountriesAsync(travelPolicy.Id, ct)
            : Array.Empty<Country>();

        if (allowedCountries.Count is 0)
        {
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_NO_ALLOWED_COUNTRIES",
                cat: SysLogCatType.Workflow,
                act: SysLogActionType.Validate,
                message: $"No allowed countries found for TravelPolicy '{travelPolicy?.Id}' associated with TravelQuote '{travelQuoteId}'",
                ent: nameof(TravelQuote),
                entId: travelQuoteId,
                note: "no_allowed_countries");
        }
        else
        {
            await _log.InformationAsync(
                evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_ALLOWED_COUNTRIES_FOUND",
                cat: SysLogCatType.App,
                act: SysLogActionType.Step,
                message: $"Found {allowedCountries.Count} allowed countries for TravelPolicy '{travelPolicy?.Id}' associated with TravelQuote '{travelQuoteId}'",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);
        }

        var allowedIso3166_Alpha2 =
            (allowedCountries ?? Array.Empty<Country>())
                .Select(c => c.IsoCode)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim().ToUpperInvariant())
                .Where(s => s.Length == 2) // only alpha-2
                .Select(s => Enum.TryParse<Iso3166_Alpha2>(s, ignoreCase: true, out var e) ? (Iso3166_Alpha2?)e : null)
                .OfType<Iso3166_Alpha2>()   // unwrap non-nulls
                .Distinct()
                .ToList();

        if (allowedIso3166_Alpha2.Count > 0)
        {
            await _log.InformationAsync(
                evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_ALLOWED_ISO3166_ALPHA2",
                cat: SysLogCatType.App,
                act: SysLogActionType.Step,
                message: $"Allowed ISO3166 Alpha-2 country codes for TravelPolicy '{travelPolicy?.Id}' associated with TravelQuote '{travelQuoteId}': {string.Join(", ", allowedIso3166_Alpha2)}",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);
        }

        var originsDestinations =
            (await _airportInfoSvc.SearchMultiAsync(
                types: new List<AirportType> { AirportType.MediumAirport, AirportType.LargeAirport },
                countries: allowedIso3166_Alpha2,
                hasIata: true,
                hasMunicipality: true,
                take: 99999,
                ct: ct))
            .Select(a => new BookingAirport(
                Code: a.IataCode!,
                Name: a.Name,
                City: a.Municipality,
                Country: a.IsoCountry.ToString()))
            .ToList();

        // Airlines
        string[] excludedAirlines = NormalizeAirlineCodes(travelPolicy?.ExcludedAirlineCodes);
        string[] includedAirlines = NormalizeAirlineCodes(travelPolicy?.IncludedAirlineCodes);

        string[] availableAirlines = excludedAirlines.Length > 0
            ? excludedAirlines
            : includedAirlines.Length > 0
                ? includedAirlines
                : Array.Empty<string>();

        var bookingAirlines = new List<BookingAirline>(availableAirlines.Length);
        foreach (var code in availableAirlines)
        {
            Airline? airline = code.Length == 2
                ? await _airlineSvc.GetByIataAsync(code, includeProgram: false, ct)
                : await _airlineSvc.GetByIcaoAsync(code, includeProgram: false, ct);

            if (airline is null) continue;

            var outCode =
                !string.IsNullOrWhiteSpace(airline.Iata) ? airline.Iata :
                !string.IsNullOrWhiteSpace(airline.Icao) ? airline.Icao : "UNK";

            bookingAirlines.Add(new BookingAirline(
                Code: outCode,
                Name: airline.Name ?? "Unknown Airline"));
        }

        // Cabins: parse strings → enum; return enum values (integers on the wire)
        var defaultCabin = TryParseCabin(travelPolicy?.DefaultFlightSeating, out var c1) ? c1 : CabinClass.Economy;
        var maxCabin = TryParseCabin(travelPolicy?.MaxFlightSeating, out var c2) ? c2 : CabinClass.First;

        // Coverage (kept if you need it later)
        var coverageType = TryParseCoverage(travelPolicy?.CabinClassCoverage, out var cov) ? cov : (CoverageType?)null;

        var config = new FlightSearchPageConfig
        {
            TenantName = orgName,
            PolicyName = policyName,
            TravelQuoteId = travelQuoteId,
            TmcAssignedId = quote.TmcAssignedId,
            EnabledOrigins = originsDestinations,
            EnabledDestinations = originsDestinations,
            AvailableAirlines = bookingAirlines,
            PreferredAirlines = includedAirlines.ToList(),
            DefaultCabin = defaultCabin,
            MaxCabin = maxCabin,
            DaysInAdvanceBookingRequired = travelPolicy?.DefaultCalendarDaysInAdvanceForFlightBooking,
            HasFixedTimes = hasFixedTimes,
            FixedDepartEarliest = earliestDepartureTime ?? string.Empty,
            FixedDepartLatest = latestDepartureTime ?? string.Empty,
            SeedDepartDate = earliestDepartureDate,
            Adults = adults
            // If you later add coverage to the DTO, set it here from coverageType.
        };

        await _log.InformationAsync(
            evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS",
            cat: SysLogCatType.App,
            act: SysLogActionType.Read,
            message: $"Generated flight search UI options for TravelQuote '{travelQuoteId}'",
            ent: nameof(TravelQuote),
            entId: travelQuoteId);

        return config;
    }

    // ---------------- PRIVATE FUNCTIONS ----------------
    private async Task ValidateRootsAsync(string orgId, string tmcId, string userId, CancellationToken ct)
    {
        if (!await _orgSvc.ExistsAsync(orgId, ct)) throw new InvalidOperationException($"Organization '{orgId}' not found.");
        if (!await _orgSvc.ExistsAsync(tmcId, ct)) throw new InvalidOperationException($"TMC org '{tmcId}' not found.");
        if (!await _userSvc.ExistsAsync(userId, ct)) throw new InvalidOperationException($"User '{userId}' not found.");
    }

    private async Task EnsureTravellerUsersExistAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        var ids = userIds.Where(s => !string.IsNullOrWhiteSpace(s))
                         .Select(s => s.Trim())
                         .Distinct(StringComparer.Ordinal)
                         .ToArray();
        if (ids.Length == 0) return;

        // Validate each id via Users DbSet for efficiency
        var found = await _db.Users.AsNoTracking().Where(u => ids.Contains(u.Id)).Select(u => u.Id).ToListAsync(ct);
        var missing = ids.Except(found, StringComparer.Ordinal).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException("Traveller user(s) not found: " + string.Join(", ", missing));
    }

    private async Task<TravelQuote> LoadAggregateTrackedAsync(string id, CancellationToken ct)
    {
        return await _db.TravelQuotes
            .Include(q => q.Organization)
            .Include(q => q.TmcAssigned)
            .Include(q => q.CreatedBy)
            .Include(q => q.Travellers).ThenInclude(t => t.User)
            .FirstAsync(q => q.Id == id, ct);
    }

    private async Task<TravelQuote> TranslateDtoAsync(TravelQuoteDto dto, CancellationToken ct)
    {
        // this should NEVER hit an error, because it's built-in to the UI
        if (!TryParseQuoteType(dto.QuoteType, out var type))
        {
            // Log then throw
            var ex = new ArgumentException($"Invalid QuoteType '{dto.QuoteType}'.");
            await _log.ErrorAsync(
                evt: "TRAVEL_QUOTE_TRANSLATE_INVALID_QUOTE_TYPE",
                cat: SysLogCatType.Workflow,
                act: SysLogActionType.Validate,
                ex: ex,
                message: $"Invalid QuoteType '{dto.QuoteType}'.",
                ent: "TravelQuoteDto",
                entId: dto.OrganizationId,
                uid: dto.CreatedByUserId,
                org: dto.OrganizationId,
                note: "invalid_quote_type");

            throw ex;
        }

        await ValidateRootsAsync(dto.OrganizationId, dto.TmcAssignedId, dto.CreatedByUserId, ct);  // hard core error handling
        await EnsureTravellerUsersExistAsync(dto.TravellerUserIds, ct);  // hard core error handling

        var q = new TravelQuote
        {
            Type = type,
            State = QuoteState.Draft,
            OrganizationId = dto.OrganizationId.Trim(),
            TmcAssignedId = dto.TmcAssignedId.Trim(),
            CreatedByUserId = dto.CreatedByUserId.Trim(),
            CoverageType = CoverageType.MostSegments,  // default; can be updated later
        };

        // foreach (var uid in dto.TravellerUserIds.Distinct(StringComparer.Ordinal))
        //     q.Travellers.Add(new TravelQuoteUser { UserId = uid });

        // De-dupe travellers, fetch policy IDs, exclude users with no policy.
        // Keep integrity lists for auditing.
        var policyIdsForIntegrity = new List<string?>();  // includes nulls
        var distinctPolicyIds = new HashSet<string>(StringComparer.Ordinal);  // non-null only
        var excludedUserIds = new List<string>();  // users dropped due to null policy

        if (dto.TravellerUserIds is not null)
        {
            var seenTravellers = new HashSet<string>(StringComparer.Ordinal);

            foreach (var uid in dto.TravellerUserIds)
            {
                if (uid is null) continue;  // skip nulls
                if (!seenTravellers.Add(uid)) continue;  // de-dupe

                string? userTravelPolicyId = await _userSvc.GetUserTravelPolicyIdAsync(uid, ct);
                string? effectivePolicyId = userTravelPolicyId
                   ?? await _orgSvc.GetOrgDefaultTravelPolicyIdAsync(dto.OrganizationId, ct);

                if (effectivePolicyId is null)
                {
                    excludedUserIds.Add(uid);              // record exclusion
                    continue;                              // drop this uid from the quote
                }

                policyIdsForIntegrity.Add(effectivePolicyId);
                distinctPolicyIds.Add(effectivePolicyId);
                q.Travellers.Add(new TravelQuoteUser { UserId = uid });
            }
        }

        // policyIdsForIntegrity: all fetched IDs (nulls included) for checks
        // distinctPolicyIds: unique non-null policy IDs
        // excludedUserIds: which users were removed due to missing policy
        await _log.WarningAsync(
            evt: "TRAVEL_QUOTE_TRANSLATE_NO_VALID_POLICIES",
            cat: SysLogCatType.Workflow,
            act: SysLogActionType.Validate,
            message: $"Quote DTO translation: {dto.TravellerUserIds?.Count() ?? 0} travellers, {distinctPolicyIds.Count} distinct non-null policies, {excludedUserIds.Count} users excluded due to missing policy.",
            ent: "TravelQuoteDto",
            entId: dto.OrganizationId,
            note: "missing_policies",
            // keep the counts for searchability in free text AND structured sinks
            overrideOutcome: null);

        foreach (var e in excludedUserIds)
        {
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_TRANSLATE_EXCLUDED_USER",
                cat: SysLogCatType.Workflow,
                act: SysLogActionType.Validate,
                message: $"Excluded traveller due to missing policy. userId={e}",
                ent: "TravelQuoteDto",
                entId: dto.OrganizationId,
                note: "excluded_missing_policy");
        }

        // obtain all travel policies referenced by travellers
        // Build pL with only currently-effective policies (UTC checks, inclusive bounds)
        if (distinctPolicyIds.Count > 1)
        {
            await _log.InformationAsync(
                evt: "TRAVEL_QUOTE_TRANSLATE_MULTI_POLICY",
                cat: SysLogCatType.Workflow,
                act: SysLogActionType.Step,
                message: $"Quote '{q.Id}' has travellers with multiple distinct policies: {string.Join(", ", distinctPolicyIds)}",
                ent: nameof(TravelQuote),
                entId: q.Id);

            await _log.InformationAsync(
                evt: "TRAVEL_QUOTE_EPHEMERAL_CREATED",
                cat: SysLogCatType.Tax, // or Data if you actually persist it immediately
                act: SysLogActionType.Create,
                message: "An ephemeral travel policy will be created to unify policies for the quote lifecycle.",
                ent: "EphemeralTravelPolicy",
                entId: q.Id, // or the new policy id once known
                note: "unify_policies");
        }

        var pL = new List<TravelPolicy>();
        var nowUtc = DateTime.UtcNow;

        foreach (var pid in distinctPolicyIds)
        {
            var policy = await _db.TravelPolicies.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pid, ct);

            if (policy is null)
            {
                await _log.WarningAsync(
                    evt: "TRAVEL_QUOTE_TRANSLATE_POLICY_NOT_FOUND",
                    cat: SysLogCatType.Workflow,
                    act: SysLogActionType.Validate,
                    message: $"Travel policy '{pid}' referenced by travellers not found in DB.",
                    ent: "TravelPolicy",
                    entId: pid,
                    note: "policy_missing");
                continue;
            }

            // Normalize to UTC kind if EF materialized as Unspecified
            DateTime? eff = policy.EffectiveFromUtc is DateTime e
                ? DateTime.SpecifyKind(e, DateTimeKind.Utc)
                : null;

            DateTime? exp = policy.ExpiresOnUtc is DateTime x
                ? DateTime.SpecifyKind(x, DateTimeKind.Utc)
                : null;

            // Rule:
            // 1) EffectiveFromUtc null OR now >= EffectiveFromUtc
            // 2) ExpiresOnUtc null OR now <= ExpiresOnUtc
            bool effectiveOk = !eff.HasValue || nowUtc >= eff.Value;
            bool expiresOk = !exp.HasValue || nowUtc <= exp.Value;

            if (effectiveOk && expiresOk)
            {
                pL.Add(policy);
            }
            else
            {
                if (!effectiveOk)
                {
                    await _log.WarningAsync(
                        evt: "TRAVEL_QUOTE_TRANSLATE_POLICY_NOT_EFFECTIVE",
                        cat: SysLogCatType.Workflow,
                        act: SysLogActionType.Validate,
                        message: $"Travel policy '{pid}' not yet effective. EffectiveFromUtc={eff:o}",
                        ent: "TravelPolicy",
                        entId: pid,
                        note: "policy_not_effective");
                }

                if (!expiresOk)
                {
                    await _log.WarningAsync(
                        evt: "TRAVEL_QUOTE_TRANSLATE_POLICY_EXPIRED",
                        cat: SysLogCatType.Workflow,
                        act: SysLogActionType.Validate,
                        message: $"Travel policy '{pid}' expired. ExpiresOnUtc={exp:o}",
                        ent: "TravelPolicy",
                        entId: pid,
                        note: "policy_expired");
                }
            }
        }

        if (pL.Count == 0)
        {
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_TRANSLATE_NO_VALID_POLICIES",
                cat: SysLogCatType.Workflow,
                act: SysLogActionType.Validate,
                message: "No travellers with valid/effective travel policies found for quote.",
                ent: nameof(TravelQuote),
                entId: q.Id,
                note: "no_effective_policies");
        }

        if (pL.Count == 1)
        {
            q.TravelPolicyId = pL[0].Id;  // single policy, assign directly
            switch (pL[0].CabinClassCoverage)
            {
                case "ALL_SEGMENTS":
                    q.CoverageType = CoverageType.AllSegments;
                    break;
                case "MOST_SEGMENTS":
                    q.CoverageType = CoverageType.MostSegments;
                    break;
                case "AT_LEAST_ONE_SEGMENT":
                    q.CoverageType = CoverageType.AtLeastOneSegment;
                    break;
                default:
                    q.CoverageType = CoverageType.MostSegments;  // default
                    break;
            }
            var orgDefaultId = await _orgSvc.GetOrgDefaultTravelPolicyIdAsync(dto.OrganizationId, ct);

            q.PolicyType = string.Equals(pL[0].Id, orgDefaultId, StringComparison.Ordinal)
                ? TravelQuotePolicyType.OrgDefault
                : TravelQuotePolicyType.UserDefined;

            q.Currency = pL[0].DefaultCurrencyCode; // set quote currency from policy (#68)
        }

        else if (pL.Count > 1)
        {
            EphemeralTravelPolicy eTravelPolicy = new()
            {
                PolicyName = $"[Ephemeral] {q.OrganizationId} @ {DateTime.UtcNow:yyyy-MM-dd HH:mm}",
                OrganizationUnifiedId = q.OrganizationId,
                DefaultCurrencyCode = await _db.Organizations
                    .AsNoTracking()
                    .Where(o => o.Id == q.OrganizationId)
                    .Select(o => o.DefaultCurrency)
                    .FirstOrDefaultAsync(ct) ?? "AUD", // fallback
                CabinClassCoverage = await _db.TravelPolicies.AsNoTracking().Where(tp => tp.OrganizationUnifiedId == q.OrganizationId)
                    .Select(tp => tp.CabinClassCoverage)
                    .FirstOrDefaultAsync(ct) ?? "MOST_SEGMENTS", // fallback
                CreatedByUserId = q.CreatedByUserId,
                CreatedAtUtc = DateTime.UtcNow,
                LastUpdatedUtc = DateTime.UtcNow,
            };
            // Merge all policies into eTravelPolicy
            //eTravelPolicy.MergeFrom(pL);
            await _log.InformationAsync(
                evt: "TRAVEL_QUOTE_EPHEMERAL_CREATED",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Create,
                message: $"Created EphemeralTravelPolicy for quote '{q.Id}' with Id '{eTravelPolicy.Id}'",
                ent: "EphemeralTravelPolicy",
                entId: eTravelPolicy.Id,
                org: q.OrganizationId);

            switch (eTravelPolicy.CabinClassCoverage)
            {
                case "ALL_SEGMENTS":
                    q.CoverageType = CoverageType.AllSegments;
                    break;
                case "MOST_SEGMENTS":
                    q.CoverageType = CoverageType.MostSegments;
                    break;
                case "AT_LEAST_ONE_SEGMENT":
                    q.CoverageType = CoverageType.AtLeastOneSegment;
                    break;
                default:
                    q.CoverageType = CoverageType.MostSegments;  // default
                    break;
            }

            q.TravelPolicyId = eTravelPolicy.Id;
            q.PolicyType = TravelQuotePolicyType.Ephemeral;
            q.Currency = eTravelPolicy.DefaultCurrencyCode;  // set quote currency from policy (#68)
        }

        return q;
    }

    private TravelPolicy ConvertEphemeralToTravelPolicy(EphemeralTravelPolicy ephemeral)
    {
        return new TravelPolicy
        {
            Id = ephemeral.Id,
            PolicyName = ephemeral.PolicyName,
            OrganizationUnifiedId = ephemeral.OrganizationUnifiedId,

            // Financial
            DefaultCurrencyCode = ephemeral.DefaultCurrencyCode,
            MaxFlightPrice = ephemeral.MaxFlightPrice,

            // Effective window
            EffectiveFromUtc = ephemeral.EffectiveFromUtc,
            ExpiresOnUtc = ephemeral.ExpiresOnUtc,

            // Auditing
            CreatedAtUtc = ephemeral.CreatedAtUtc,
            LastUpdatedUtc = ephemeral.LastUpdatedUtc,
            // CreatedByUserId exists only on EphemeralTravelPolicy

            // Flight
            DefaultFlightSeating = ephemeral.DefaultFlightSeating,
            MaxFlightSeating = ephemeral.MaxFlightSeating,
            IncludedAirlineCodes = ephemeral.IncludedAirlineCodes?.ToArray() ?? Array.Empty<string>(),
            ExcludedAirlineCodes = ephemeral.ExcludedAirlineCodes?.ToArray() ?? Array.Empty<string>(),
            CabinClassCoverage = ephemeral.CabinClassCoverage,
            NonStopFlight = ephemeral.NonStopFlight,

            // Accommodation
            MaxHotelNightlyRate = ephemeral.MaxHotelNightlyRate,
            DefaultHotelRoomType = ephemeral.DefaultHotelRoomType,
            MaxHotelRoomType = ephemeral.MaxHotelRoomType,
            IncludedHotelChains = ephemeral.IncludedHotelChains?.ToArray() ?? Array.Empty<string>(),
            ExcludedHotelChains = ephemeral.ExcludedHotelChains?.ToArray() ?? Array.Empty<string>(),
            HotelBookingTimeAvailableFrom = ephemeral.HotelBookingTimeAvailableFrom,
            HotelBookingTimeAvailableTo = ephemeral.HotelBookingTimeAvailableTo,
            EnableSaturdayHotelBookings = ephemeral.EnableSaturdayHotelBookings,
            EnableSundayHotelBookings = ephemeral.EnableSundayHotelBookings,

            // Taxi / Ride-hail
            MaxTaxiFarePerRide = ephemeral.MaxTaxiFarePerRide,
            IncludedTaxiVendors = ephemeral.IncludedTaxiVendors?.ToArray() ?? Array.Empty<string>(),
            ExcludedTaxiVendors = ephemeral.ExcludedTaxiVendors?.ToArray() ?? Array.Empty<string>(),
            MaxTaxiSurgeMultiplier = ephemeral.MaxTaxiSurgeMultiplier,

            // Train
            DefaultTrainClass = ephemeral.DefaultTrainClass,
            MaxTrainClass = ephemeral.MaxTrainClass,
            MaxTrainPrice = ephemeral.MaxTrainPrice,
            IncludedRailOperators = ephemeral.IncludedRailOperators?.ToArray() ?? Array.Empty<string>(),
            ExcludedRailOperators = ephemeral.ExcludedRailOperators?.ToArray() ?? Array.Empty<string>(),

            // Hire car
            MaxCarHireDailyRate = ephemeral.MaxCarHireDailyRate,
            AllowedCarHireClasses = ephemeral.AllowedCarHireClasses?.ToArray() ?? Array.Empty<string>(),
            IncludedCarHireVendors = ephemeral.IncludedCarHireVendors?.ToArray() ?? Array.Empty<string>(),
            ExcludedCarHireVendors = ephemeral.ExcludedCarHireVendors?.ToArray() ?? Array.Empty<string>(),
            RequireInclusiveInsurance = ephemeral.RequireInclusiveInsurance,
            DefaultCarClass = ephemeral.DefaultCarClass,
            MaxCarClass = ephemeral.MaxCarClass,
            MaxCarDailyRate = ephemeral.MaxCarDailyRate,

            // Bus / Coach
            MaxBusFarePerTicket = ephemeral.MaxBusFarePerTicket,
            IncludedBusOperators = ephemeral.IncludedBusOperators?.ToArray() ?? Array.Empty<string>(),
            ExcludedBusOperators = ephemeral.ExcludedBusOperators?.ToArray() ?? Array.Empty<string>(),

            // SIM / eSIM
            MaxSimBundlePrice = ephemeral.MaxSimBundlePrice,
            MinSimDataGb = ephemeral.MinSimDataGb,
            MinSimValidityDays = ephemeral.MinSimValidityDays,
            IncludedSimVendors = ephemeral.IncludedSimVendors?.ToArray() ?? Array.Empty<string>(),
            ExcludedSimVendors = ephemeral.ExcludedSimVendors?.ToArray() ?? Array.Empty<string>(),

            // Activities
            MaxActivityPricePerPerson = ephemeral.MaxActivityPricePerPerson,
            IncludedActivityProviders = ephemeral.IncludedActivityProviders?.ToArray() ?? Array.Empty<string>(),
            ExcludedActivityProviders = ephemeral.ExcludedActivityProviders?.ToArray() ?? Array.Empty<string>(),
            AllowHighRiskActivities = ephemeral.AllowHighRiskActivities,

            // Booking time rules
            FlightBookingTimeAvailableFrom = ephemeral.FlightBookingTimeAvailableFrom,
            FlightBookingTimeAvailableTo = ephemeral.FlightBookingTimeAvailableTo,
            EnableSaturdayFlightBookings = ephemeral.EnableSaturdayFlightBookings,
            EnableSundayFlightBookings = ephemeral.EnableSundayFlightBookings,
            DefaultCalendarDaysInAdvanceForFlightBooking = ephemeral.DefaultCalendarDaysInAdvanceForFlightBooking,

            // Geography allow/deny
            RegionIds = ephemeral.RegionIds?.ToArray() ?? Array.Empty<int>(),
            ContinentIds = ephemeral.ContinentIds?.ToArray() ?? Array.Empty<int>(),
            CountryIds = ephemeral.CountryIds?.ToArray() ?? Array.Empty<int>(),
            DisabledCountryIds = ephemeral.DisabledCountryIds?.ToArray() ?? Array.Empty<int>(),

            // Duration thresholds: cabin
            MaxFlightSeatingAt6Hours = ephemeral.MaxFlightSeatingAt6Hours,
            MaxFlightSeatingAt8Hours = ephemeral.MaxFlightSeatingAt8Hours,
            MaxFlightSeatingAt10Hours = ephemeral.MaxFlightSeatingAt10Hours,
            MaxFlightSeatingAt14Hours = ephemeral.MaxFlightSeatingAt14Hours,

            // Duration thresholds: price
            MaxFlightPriceAt6Hours = ephemeral.MaxFlightPriceAt6Hours,
            MaxFlightPriceAt8Hours = ephemeral.MaxFlightPriceAt8Hours,
            MaxFlightPriceAt10Hours = ephemeral.MaxFlightPriceAt10Hours,
            MaxFlightPriceAt14Hours = ephemeral.MaxFlightPriceAt14Hours,

            // Approvals
            AutoApproveToPolicyLimit = ephemeral.AutoApproveToPolicyLimit,
            RequireManagerApprovalToPolicyLimit = ephemeral.RequireManagerApprovalToPolicyLimit,
            L1ApprovalRequired = ephemeral.L1ApprovalRequired,
            L1ApprovalAmount = ephemeral.L1ApprovalAmount,
            L2ApprovalRequired = ephemeral.L2ApprovalRequired,
            L2ApprovalAmount = ephemeral.L2ApprovalAmount,
            L3ApprovalRequired = ephemeral.L3ApprovalRequired,
            L3ApprovalAmount = ephemeral.L3ApprovalAmount,
            BillingContactApprovalToPolicyLimit = ephemeral.BillingContactApprovalToPolicyLimit,
            BillingContactApprovalAbovePolicyLimit = ephemeral.BillingContactApprovalAbovePolicyLimit
        };
    }

    private static string[] NormalizeAirlineCodes(IEnumerable<string>? codes) =>
        (codes ?? Array.Empty<string>())
            .Select(s => s?.Trim().ToUpperInvariant())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()!;

    private static string CabinLabel(CabinClass c) => c switch
    {
        CabinClass.Economy => "Economy",
        CabinClass.PremiumEconomy => "Premium Economy",
        CabinClass.Business => "Business",
        CabinClass.First => "First",
        _ => c.ToString()
    };

    private static string CoverageLabel(CoverageType c) => c switch
    {
        CoverageType.MostSegments => "Most Segments",
        CoverageType.AllSegments => "All Segments",
        CoverageType.AtLeastOneSegment => "At Least One Segment",
        _ => c.ToString()
    };

    // Accepts "ECONOMY", "economy", "PREMIUM_ECONOMY", "PremiumEconomy", with spaces/underscores ignored.
    private static bool TryParseCabin(string? s, out CabinClass cabin)
    {
        cabin = CabinClass.Economy;
        if (string.IsNullOrWhiteSpace(s)) return false;

        var key = s.Trim().ToUpperInvariant().Replace("_", "").Replace(" ", "");
        return key switch
        {
            "ECONOMY" => (cabin = CabinClass.Economy) is var _ && true,
            "PREMIUMECONOMY" => (cabin = CabinClass.PremiumEconomy) is var _ && true,
            "BUSINESS" => (cabin = CabinClass.Business) is var _ && true,
            "FIRST" => (cabin = CabinClass.First) is var _ && true,
            _ => false
        };
    }

    private static bool TryParseCoverage(string? s, out CoverageType coverage)
    {
        coverage = CoverageType.MostSegments;
        if (string.IsNullOrWhiteSpace(s)) return false;

        var key = s.Trim().ToUpperInvariant().Replace(" ", "").Replace("-", "").Replace("_", "");
        return key switch
        {
            "MOSTSEGMENTS" => (coverage = CoverageType.MostSegments) is var _ && true,
            "ATLEASTONESEGMENT" => (coverage = CoverageType.AtLeastOneSegment) is var _ && true,
            "ALLSEGMENTS" => (coverage = CoverageType.AllSegments) is var _ && true,
            _ => false
        };
    }

    /// <summary>
    /// Build an AmadeusFlightOfferSearch request from a TravelQuote. This is never going to do a search for the return flight,
    /// it will only built the search request object per direction, as it's used in the flight search page and in the quote generation process.
    /// So the page requesting will need to identify which flight direction it's searching for. The TravelQuote contains both directions
    /// if it's a return trip in the enum value, and the builder will create the direction accordingly, one at a time. This is the unique selling
    /// point of this builder, as traditional searches require the full round-trip details to be specified at once.
    /// </summary>
    /// <param name="quote">The TravelQuote to build the search request from.</param>
    /// <param name="returnTrip">If true, build the return trip direction; otherwise, build the outbound trip direction.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns></returns>
    public async Task<AmadeusFlightOfferSearch> BuildAmadeusFlightOfferSearchFromQuote(TravelQuote quote, bool returnTrip, CancellationToken ct = default)
    {
        // currency code (handled directly in the return statement)

        // origin destinations
        List<OriginDestination> originDestinations = new List<OriginDestination>();
        switch (returnTrip)
        {
            case false:  // outbound
                originDestinations.Add(new OriginDestination
                {
                    Id = "1",
                    OriginLocationCode = quote.OriginIataCode!,
                    DestinationLocationCode = quote.DestinationIataCode!,
                    DateTimeRange = new DepartureDateTimeRange
                    {
                        Date = quote.DepartureDate!,
                        Time = string.IsNullOrEmpty(quote.DepartEarliestTime)
                            ? "00:00:00"
                            : quote.DepartEarliestTime
                    }
                });
                break;
            case true:  // return
                originDestinations.Add(new OriginDestination
                {
                    Id = "2",
                    OriginLocationCode = quote.DestinationIataCode!,
                    DestinationLocationCode = quote.OriginIataCode!,
                    DateTimeRange = new DepartureDateTimeRange
                    {
                        Date = quote.ReturnDate!,
                        Time = string.IsNullOrEmpty(quote.ReturnEarliestTime)
                            ? "00:00:00"
                            : quote.ReturnEarliestTime
                    }
                });
                break;
        }

        // travelers
        var count = quote.Travellers?.Count ?? 0;
        var travelers = Enumerable.Range(1, count)
            .Select(i => new Traveler
            {
                Id = i.ToString(CultureInfo.InvariantCulture),
                TravelerType = "ADULT",
                FareOptions = new List<string> { "STANDARD" },
            })
            .ToList();

        // search criteria
        List<CabinRestriction> cabinRestrictions = new List<CabinRestriction>
        {
            new CabinRestriction
            {
                Cabin = quote.CabinClass switch
                {
                    CabinClass.Economy          => "ECONOMY",
                    CabinClass.PremiumEconomy   => "PREMIUM_ECONOMY",
                    CabinClass.Business         => "BUSINESS",
                    CabinClass.First            => "FIRST",
                    _                           => "ECONOMY"
                },
                Coverage = quote.CoverageType switch
                {
                    CoverageType.MostSegments       => "MOST_SEGMENTS",
                    CoverageType.AtLeastOneSegment  => "AT_LEAST_ONE_SEGMENT",
                    CoverageType.AllSegments        => "ALL_SEGMENTS",
                    _                               => "MOST_SEGMENTS"
                },
                OriginDestinationIds = quote.TripType switch
                {
                    TripType.OneWay => new List<string> { "1" },
                    TripType.Return => new List<string> { "1", "2" },
                    _               => new List<string> { "1" }
                }
            }
        };

        // is there any TravelPolicy.ExcludedAirlineCodes to consider?
        // Excluded airlines take precedence over included
        List<string>? excludedAirlineCodes = await GetExcludedAirlinesFromPolicyAsync(quote.TravelPolicyId, quote.PolicyType, ct);

        List<string> includedAirlineCodes;
        if (excludedAirlineCodes is not null && excludedAirlineCodes.Count > 0)
        {
            includedAirlineCodes = new();  // no preference when exclusions are defined
        }
        else
        {
            // build included airline codes from quote selections and alliances
            var included = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // From explicit selections
            if (quote.SelectedAirlines is not null)
                included.UnionWith(quote.SelectedAirlines.Where(s => !string.IsNullOrWhiteSpace(s)));

            // From alliances (single round-trip)
            //if (quote.Alliances is not null && quote.Alliances.Count > 0)
            if (quote?.Alliances?.Count > 0)
            {
                var allianceCodes = await _db.Airlines
                    .AsNoTracking()
                    .Where(a => a.Iata != null && quote.Alliances.Contains(a.Alliance))
                    .Select(a => a.Iata!)                 // IATA is 2-letter
                    .Distinct()
                    .ToListAsync(ct);

                included.UnionWith(allianceCodes);
            }

            includedAirlineCodes = included.Count == 0 ? new() : included.ToList(); // empty => no preference
            if (includedAirlineCodes.Count > 150)
            {
                // Amadeus max is 150 included carriers; too many selected → ignore
                includedAirlineCodes = new();
            }
        }

        CarrierRestriction carrierRestriction = new CarrierRestriction
        {
            BlacklistedInEUAllowed = false
        };
        if (excludedAirlineCodes is not null && excludedAirlineCodes.Count > 0)
        {
            carrierRestriction.ExcludedCarrierCodes = excludedAirlineCodes;
        }
        else if (includedAirlineCodes.Count > 0)
        {
            carrierRestriction.IncludedCarrierCodes = includedAirlineCodes;
        }

        AmadeusFlightOfferSearch amadeusFlightOfferSearchQuery = new AmadeusFlightOfferSearch
        {
            CurrencyCode = string.IsNullOrWhiteSpace(quote?.Currency) ? "AUD" : quote!.Currency,
            OriginDestinations = originDestinations,
            Travelers = travelers,
            Sources = new List<string> { "GDS" },
            SearchCriteria = new SearchCriteria
            {
                MaxFlightOffers = 250,
                Filters = new FlightFilters
                {
                    CabinRestrictions = cabinRestrictions,
                    CarrierRestrictions = carrierRestriction
                }
            }
        };

        return amadeusFlightOfferSearchQuery;
    }

    /// <summary>
    /// Get flight search results for a TravelQuote, applying organization markup/fees as needed. This will need to be stored in a custom return with
    /// the number of results, any error message, List<FlightViewOption> is nullable if no results found. Notice to say "if there are no results"
    /// and any additional information the UI needs, this will be expanded later.
    /// </summary>
    /// <param name="travelQuoteId"></param>
    /// <param name="results"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<FlightSearchResponse> GetFlightSearchResultsAsync(string travelQuoteId, AmadeusFlightOfferSearchResult results, CancellationToken ct = default)
    {
        // validate results
        if (results?.Data == null || results.Data.Count == 0)
        {
            await _log.InformationAsync(
                evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_NO_DATA",
                cat: SysLogCatType.App,
                act: SysLogActionType.Read,
                message: $"No flight search results data found for TravelQuote '{travelQuoteId}'",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);
            return new FlightSearchResponse
            {
                QuoteId = travelQuoteId,
                StatusCode = HttpStatusCode.NoContent,
                Message = $"No flight search results data found for TravelQuote '{travelQuoteId}'",
                // Options not set → stays []
                MoreResultsAvailable = false
            };
        }

        // get the TravelQuote
        TravelQuote? quote = await GetByIdAsync(travelQuoteId, ct);
        if (quote is null)
        {
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_QUOTE_NOT_FOUND",
                cat: SysLogCatType.App,
                act: SysLogActionType.Read,
                message: $"TravelQuote '{travelQuoteId}' not found when retrieving flight search results",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);
            return new FlightSearchResponse
            {
                QuoteId = travelQuoteId,
                StatusCode = HttpStatusCode.NoContent,
                Message = $"TravelQuote '{travelQuoteId}' not found when retrieving flight search results",
                // Options not set → stays []
                MoreResultsAvailable = false
            };
        }

        // we also need the markup percentage for the quote's organization
        OrgFeesMarkupDto? orgFees = await _orgService.GetOrgPnrServiceFeesAsync(quote.OrganizationId, ct);

        if (orgFees is null)
        {
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_ORG_FEES_NOT_FOUND",
                cat: SysLogCatType.App,
                act: SysLogActionType.Read,
                message: $"Organization fees/markup not found for Organization '{quote.OrganizationId}' when retrieving flight search results for TravelQuote '{travelQuoteId}'",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);
            return new FlightSearchResponse
            {
                QuoteId = travelQuoteId,
                StatusCode = HttpStatusCode.NoContent,
                Message = $"Organization fees/markup not found for Organization '{quote.OrganizationId}' when retrieving flight search results for TravelQuote '{travelQuoteId}'",
                // Options not set → stays []
                MoreResultsAvailable = false
            };
        }

        // fees
        decimal markupPercentage = 0m;
        decimal markupAmount = 0m;
        ServiceFeeType feeType = ServiceFeeType.None;

        if (orgFees.TravelFeeType != ServiceFeeType.None)
        {
            // apply markup to each flight option price
            switch (orgFees.TravelFeeType)
            {
                case ServiceFeeType.MarkupOnly:
                    // markup is percentage-based
                    markupPercentage = orgFees.TravelMarkupPercent != 0m ? orgFees.TravelMarkupPercent : 0m;
                    feeType = ServiceFeeType.MarkupOnly;
                    break;
                case ServiceFeeType.PerItemFeeOnly:
                    // markup is amount-based
                    markupAmount = orgFees.TravelPerItemFee != 0m ? orgFees.TravelPerItemFee : 0m;
                    feeType = ServiceFeeType.PerItemFeeOnly;
                    break;

                case ServiceFeeType.MarkupAndPerItemFee:
                    // both markup and amount-based
                    markupPercentage = orgFees.TravelMarkupPercent != 0m ? orgFees.TravelMarkupPercent : 0m;
                    markupAmount = orgFees.TravelPerItemFee != 0m ? orgFees.TravelPerItemFee : 0m;
                    feeType = ServiceFeeType.MarkupAndPerItemFee;
                    break;
            }
        }
        else if (orgFees.FlightFeeType != ServiceFeeType.None)
        {
            // apply markup to each flight option price
            switch (orgFees.FlightFeeType)
            {
                case ServiceFeeType.MarkupOnly:
                    // markup is percentage-based
                    markupPercentage = orgFees.FlightMarkupPercent != 0m ? orgFees.FlightMarkupPercent : 0m;
                    feeType = ServiceFeeType.MarkupOnly;
                    break;
                case ServiceFeeType.PerItemFeeOnly:
                    // markup is amount-based
                    markupAmount = orgFees.FlightPerItemFee != 0m ? orgFees.FlightPerItemFee : 0m;
                    feeType = ServiceFeeType.PerItemFeeOnly;
                    break;

                case ServiceFeeType.MarkupAndPerItemFee:
                    // both markup and amount-based
                    markupPercentage = orgFees.FlightMarkupPercent != 0m ? orgFees.FlightMarkupPercent : 0m;
                    markupAmount = orgFees.FlightPerItemFee != 0m ? orgFees.FlightPerItemFee : 0m;
                    feeType = ServiceFeeType.MarkupAndPerItemFee;
                    break;
            }
        }
        else
        {
            // no markup - did someone do something stupid?
            await _log.WarningAsync(
                evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_NO_ORG_MARKUP",
                cat: SysLogCatType.App,
                act: SysLogActionType.Read,
                message: $"No organization markup defined for Organization '{quote.OrganizationId}' when retrieving flight search results for TravelQuote '{travelQuoteId}'",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);
        }

        // establish Amadeus results data
        List<FlightOffer> resultsData;

        // double check and fail fast
        if (results.Data is not null)
        {
            resultsData = results.Data;
        }
        else
        {
            return new FlightSearchResponse
            {
                QuoteId = travelQuoteId,
                StatusCode = HttpStatusCode.NoContent,
                Message = $"No flight search results data found for TravelQuote '{travelQuoteId}'",
                // Options not set → stays []
                MoreResultsAvailable = false
            };    
        }

        // init FlightViewOptions
        List<FlightViewOption> flightViewOptions = new List<FlightViewOption>(resultsData.Count);

        // process each flight offer
        foreach (var flightOffer in results.Data)
        {
            // metadata
            string origin = quote?.OriginIataCode ?? string.Empty;
            string destination = quote?.DestinationIataCode ?? string.Empty;

            // base amount for flight offer
            var costBaseAmount = decimal.TryParse(flightOffer?.Price?.Total, NumberStyles.Number, CultureInfo.InvariantCulture, out var amt)
                ? amt
                : 0m;

            // calculate cost
            decimal costMarkup = feeType switch
            {
                ServiceFeeType.MarkupOnly => costBaseAmount * (1 + markupPercentage / 100),
                ServiceFeeType.PerItemFeeOnly => costBaseAmount + markupAmount,
                ServiceFeeType.MarkupAndPerItemFee => costBaseAmount * (1 + markupPercentage / 100) + markupAmount,
                _ => costBaseAmount
            };

            // set currency code
            string currencyCode = quote?.Currency ?? "AUD";

            // hold a list of all the cabins being flown here:
            List<string> _cabins = new();

            // stops the number in between leg counts
            int stops = (flightOffer?.Itineraries?.First().Segments?.Count() ?? 0) - 1;

            // create legs of the flight
            List<FlightLeg> legs = new List<FlightLeg>();

            foreach (var itinerary in flightOffer?.Itineraries ?? Enumerable.Empty<Itinerary>())
            {
                foreach (var seg in itinerary.Segments ?? Enumerable.Empty<Segment>())
                {
                    var fLeg = new FlightLeg();

                    var segmentId = seg.Id;

                    fLeg.Carrier = new Carrier(
                        seg.CarrierCode!,
                        seg.CarrierCode!,  // TODO: lookup name from code
                        string.Empty);

                    fLeg.FlightNumber = seg.Number!;

                    fLeg.Origin = seg.Departure?.IATACode!;
                    fLeg.OriginTerminal = seg.Departure?.Terminal!;

                    fLeg.Destination = seg.Arrival?.IATACode!;
                    fLeg.DestinationTerminal = seg.Arrival?.Terminal!;

                    fLeg.Depart = DateTime.Parse(seg.Departure?.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                    fLeg.Arrive = DateTime.Parse(seg.Arrival?.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

                    fLeg.Equipment = seg.Aircraft?.Code!;
                    fLeg.EquipmentName =
                        seg.Aircraft?.Code is { Length: > 0 } c
                        && results.Dictionaries?.Aircraft is { } ac
                        && ac.TryGetValue(c, out var n)
                            ? n
                            : seg.Aircraft?.Code ?? "UNKNOWN";

                    fLeg.SeatLayout = string.Empty;  //TODO: Find out how the seat layouts can be identified PER aircraft PER airline later

                    // map the cabin class for the leg of the flight
                    var _cabin =
                        flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .Cabin
                        ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(_cabin))
                        _cabins.Add(_cabin);

                    fLeg.CabinClass = _cabin;




                    

                    legs.Add(new FlightLeg
                    {
                        Carrier = new Carrier(
                            seg.CarrierCode!,
                            seg.CarrierCode!,  // TODO: lookup name from code
                            string.Empty),
                        FlightNumber = seg.Number!,
                        Origin = seg.Departure?.IATACode!,
                        Destination = seg.Arrival?.IATACode!,
                        Depart = DateTime.Parse(seg.Departure?.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Arrive = DateTime.Parse(seg.Arrival?.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                        Equipment = seg.Aircraft?.Code!,
                        SeatLayout = string.Empty,
                        // CabinClass = flightOffer?
                        //     .TravelerPricings?
                        //     .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                        //     .FirstOrDefault(fds => fds.SegmentId == segmentId)
                        //     ?.Cabin ?? "ECONOMY",
                        Amenities = new Amenities
                        {
                            Wifi = false,
                            Power = false,
                            Usb = false,
                            Ife = false,
                            Meal = false,
                            LieFlat = false,
                            ExtraLegroom = false,
                            Lounge = false,
                            PriorityBoarding = false,
                            CheckedBag = true,
                            Alcohol = false,
                        },
                        Layover = null,  // TODO: null has to be calculated AFTER the number of legs are known
                                         // and calculated in the FlightViewOption object
                })
            }



            foreach (var leg in flightOffer?.Itineraries?.First().Segments ?? Enumerable.Empty<Segment>())
            {
                var segmentId = int.Parse(leg.Id!, CultureInfo.InvariantCulture);

                legs.Add(new FlightLeg
                {
                    Carrier = new Carrier(
                        leg.CarrierCode!,
                        leg.CarrierCode!,  // TODO: lookup name from code
                        string.Empty),
                    FlightNumber = leg.Number!,
                    Origin = leg.Departure?.IATACode!,
                    Destination = leg.Arrival?.IATACode!,
                    Depart = DateTime.Parse(leg.Departure?.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    Arrive = DateTime.Parse(leg.Arrival?.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                    Equipment = leg.Aircraft?.Code!,
                    SeatLayout = string.Empty,
                    // CabinClass = flightOffer?
                    //     .TravelerPricings?
                    //     .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                    //     .FirstOrDefault(fds => fds.SegmentId == segmentId)
                    //     ?.Cabin ?? "ECONOMY",
                    Amenities = new Amenities
                    {
                        Wifi = false,
                        Power = false,
                        Usb = false,
                        Ife = false,
                        Meal = false,
                        LieFlat = false,
                        ExtraLegroom = false,
                        Lounge = false,
                        PriorityBoarding = false,
                        CheckedBag = true,
                        Alcohol = false,
                    },
                    Layover = null,  // TODO: null has to be calculated AFTER the number of legs are known
                                     // and calculated in the FlightViewOption object
                });
            }

            string cabin = quote?.CabinClass switch
            {
                CabinClass.Economy => "Economy",
                CabinClass.PremiumEconomy => "Premium Economy",
                CabinClass.Business => "Business",
                CabinClass.First => "First",
                _ => "Economy"
            };

            flightViewOptions.Add(new FlightViewOption
            {
                Origin = quote?.OriginIataCode!,
                Destination = quote?.DestinationIataCode!,
                DepartTime = DateTime.Parse(flightOffer?.Itineraries?.First().Segments?.First().Departure!.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ArriveTime = DateTime.Parse(flightOffer?.Itineraries?.First().Segments?.First().Departure!.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                    + XmlConvert.ToTimeSpan(flightOffer?.Itineraries?.First().Duration!),
                Price = CalcTotalFromOrgFeesMarkupDto(costBaseAmount, feeType, markupPercentage, markupAmount),
                Currency = quote?.Currency!,
                Cabin = cabin,
                Stops = flightOffer?.Itineraries?.First().Segments?.Count() ?? 0,
                QuoteId = travelQuoteId,
                Amenities = new Amenities
                {
                    Wifi = false,
                    Power = false,
                    Usb = false,
                    Ife = false,
                    Meal = false,
                    LieFlat = false,
                    ExtraLegroom = false,
                    Lounge = false,
                    PriorityBoarding = false,
                    CheckedBag = true,
                    Alcohol = false,
                },
                Legs = legs,
                BaggageText = string.Empty,
                ChangePolicy = string.Empty,
                RefundPolicy = string.Empty,
                SeatPolicy = string.Empty,

            });

            // await _log.InformationAsync(
            //     evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_OFFER",
            //     cat: SysLogCatType.App,
            //     act: SysLogActionType.Read,
            //     message: $"Flight offer found for TravelQuote '{travelQuoteId}': OfferId='{flightOffer.Id}', Price='{flightOffer.Price.Total} {flightOffer.Price.Currency}'",
            //     ent: nameof(TravelQuote),
            //     entId: travelQuoteId);
        }

        return new FlightSearchResponse
        {
            QuoteId = travelQuoteId,
            StatusCode = flightViewOptions.Count > 0 ? HttpStatusCode.OK : HttpStatusCode.NoContent,
            Message = flightViewOptions.Count > 0 ? "Flight search results retrieved successfully." : "No flight search results found.",
            Options = flightViewOptions,
            MoreResultsAvailable = quote?.TripType == TripType.Return ? true : false
        };
    }
    
    private decimal CalcTotalFromOrgFeesMarkupDto(decimal baseAmount, ServiceFeeType feeType, decimal? pct, decimal? flat)
    {
        decimal total = baseAmount;
        decimal pctVal = (pct is > 0m) ? pct.Value : 0m;
        decimal flatVal = (flat is > 0m) ? flat.Value : 0m;

        switch (feeType)
        {
            case ServiceFeeType.MarkupOnly:
                total += baseAmount * pctVal;
                break;

            case ServiceFeeType.PerItemFeeOnly:
                total += flatVal;
                break;

            case ServiceFeeType.MarkupAndPerItemFee:
                total += baseAmount * pctVal;
                total += flatVal;
                break;
        }

        return total;
    }
}
