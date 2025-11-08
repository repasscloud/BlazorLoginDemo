// Services/Travel/TravelQuoteService.cs
using System.Globalization;
using System.Text.Json;
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
        // establish Amadeus results data
        List<FlightOffer> resultsData;

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
        else
        {
            resultsData = results.Data;
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
                Message = $"Fees not found for Organization '{quote.OrganizationId}' when retrieving flight search results for TravelQuote '{travelQuoteId}'. Please contact your PNR admin for support.",
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

        // init FlightViewOptions list with count of results from Amadeus
        List<FlightViewOption> flightViewOptions = new List<FlightViewOption>(resultsData.Count);

        // process each flight offer
        foreach (var flightOffer in resultsData)
        {
            // metadata
            string origin = quote?.OriginIataCode ?? string.Empty;
            string destination = quote?.DestinationIataCode ?? string.Empty;

            // base amount for flight offer
            var costBaseAmount = decimal.TryParse(flightOffer?.Price?.GrandTotal, NumberStyles.Number, CultureInfo.InvariantCulture, out var amt)
                ? amt
                : 0m;

            // calculate cost (what the client will pay) after markup/fees
            decimal costMarkedUp = feeType switch
            {
                ServiceFeeType.MarkupOnly => costBaseAmount * (1 + markupPercentage / 100),
                ServiceFeeType.PerItemFeeOnly => costBaseAmount + markupAmount,
                ServiceFeeType.MarkupAndPerItemFee => costBaseAmount * (1 + markupPercentage / 100) + markupAmount,
                _ => costBaseAmount
            };

            // set currency code
            string currencyCode = quote?.Currency ?? "AUD";
            string currencySymbol = "$";

            // set currency symbol
            switch (currencyCode.ToUpperInvariant())
            {
                case "AUD":
                    currencySymbol = "A$";
                    break;
                case "USD":
                    currencySymbol = "US$";
                    break;
                case "CAD":
                    currencySymbol = "C$";
                    break;
                case "NZD":
                    currencySymbol = "NZ$";
                    break;
                case "EUR":
                    currencySymbol = "€";
                    break;
                case "GBP":
                    currencySymbol = "£";
                    break;
                case "JPY":
                    currencySymbol = "¥";
                    break;
                default:
                    break;
            }

            // hold a list of all the cabins being flown here:
            List<string> _cabins = new();

            // stops the number in between leg counts
            int stops = Math.Max(
                (flightOffer?.Itineraries?
                    .FirstOrDefault()?
                    .Segments?
                    .Count() ?? 0) - 1,
                0);

            // create legs of the flight
            List<FlightLeg> legs = new List<FlightLeg>();

            foreach (var itinerary in flightOffer?.Itineraries ?? Enumerable.Empty<Itinerary>())
            {
                var segs = itinerary.Segments?.ToList() ?? new List<Segment>();

                for (int i = 0; i < segs.Count; i++)
                {
                    var seg = segs[i];

                    var fLeg = new FlightLeg();

                    var segmentId = seg.Id;

                    var durationText = seg.duration ?? "PT0H0M";
                    var segDuration = XmlConvert.ToTimeSpan(durationText);

                    fLeg.DurationText = $"{(int)segDuration.TotalHours}h {segDuration.Minutes}m";
                    fLeg.Duration = segDuration;

                    fLeg.Carrier = new Carrier(
                        seg.CarrierCode != null ? seg.CarrierCode : "XX",
                        seg.CarrierCode != null ? seg.CarrierCode : "Unknown",  // TODO: lookup name from code
                        $"https://raw.githubusercontent.com/repasscloud/IATAScraper/refs/heads/main/airline_vectors/{seg.CarrierCode!.ToUpper()}.svg");

                    // operating carrier (if different)
                    fLeg.OperatingCarrierCode = seg.Operating?.CarrierCode ?? seg.CarrierCode ?? null;

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

                    // add the cabin to the list of cabins (will filter later)
                    if (!string.IsNullOrWhiteSpace(_cabin))
                        _cabins.Add(_cabin);

                    fLeg.CabinClass = _cabin;


                    // checked bags
                    fLeg.CheckedBagsAllowed = flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .IncludedCheckedBags?.Quantity ?? 0;

                    fLeg.CheckedBagsWeight = flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .IncludedCheckedBags?.Weight ?? null;

                    fLeg.CheckedBagsWeightUnit = flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .IncludedCheckedBags?.WeightUnit ?? string.Empty;

                    // cabin bags
                    fLeg.CabinBagsAllowed = flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .IncludedCabinBags?.Quantity ?? 0;

                    fLeg.CabinBagsWeight = flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .IncludedCabinBags?.Weight ?? null;

                    fLeg.CabinBagsWeightUnit = flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .IncludedCabinBags?.WeightUnit ?? string.Empty;


                    // map amenities for the leg of the flight
                    List<Amenity> Amenities = new List<Amenity>();

                    // get current amenities (if any) for segment
                    var _amenities =
                        flightOffer?.TravelerPricings?
                            .SelectMany(tp => tp.FareDetailsBySegment ?? Enumerable.Empty<FareDetailBySegment>())
                            .FirstOrDefault(fds => fds.SegmentId == segmentId)?
                            .Amenities
                        ?? null;

                    if (_amenities is not null)
                    {
                        foreach (var a in _amenities)
                        {
                            Amenity _amenity = new Amenity();

                            switch (a.AmenityType)
                            {
                                case "BAGGAGE":
                                    _amenity.Type = AmenityType.BAGGAGE;
                                    _amenity.Name = a.Description switch
                                    {
                                        "PRE PAID BAGGAGE" => "Pre-paid Baggage",
                                        "1PC MAX 23KG 158LCM EACH" => "1PC 23KG 158LCM",
                                        "1PC MAX 15LB 7KG 115LCM" => "1PC 7KG 115LCM",
                                        _ => a.Description!
                                    };
                                    _amenity.SvgPath = a.Description switch
                                    {
                                        "PRE PAID BAGGAGE" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-luggage\" viewBox=\"0 0 16 16\">\n  <path d=\"M2.5 1a.5.5 0 0 0-.5.5V5h-.5A1.5 1.5 0 0 0 0 6.5v7a1.5 1.5 0 0 0 1 1.415v.335a.75.75 0 0 0 1.5 0V15H4v-1H1.5a.5.5 0 0 1-.5-.5v-7a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 .5.5V7h1v-.5A1.5 1.5 0 0 0 6.5 5H6V1.5a.5.5 0 0 0-.5-.5zM5 5H3V2h2z\"/>\n  <path d=\"M3 7.5a.5.5 0 0 0-1 0v5a.5.5 0 0 0 1 0zM11 6a1.5 1.5 0 0 1 1.5 1.5V8h2A1.5 1.5 0 0 1 16 9.5v5a1.5 1.5 0 0 1-1.5 1.5h-8A1.5 1.5 0 0 1 5 14.5v-5A1.5 1.5 0 0 1 6.5 8h2v-.5A1.5 1.5 0 0 1 10 6zM9.5 7.5V8h2v-.5A.5.5 0 0 0 11 7h-1a.5.5 0 0 0-.5.5M6 9.5v5a.5.5 0 0 0 .5.5H7V9h-.5a.5.5 0 0 0-.5.5m7 5.5V9H8v6zm1.5 0a.5.5 0 0 0 .5-.5v-5a.5.5 0 0 0-.5-.5H14v6z\"/>\n</svg>",
                                        "1PC MAX 23KG 158LCM EACH" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-luggage\" viewBox=\"0 0 16 16\">\n  <path d=\"M2.5 1a.5.5 0 0 0-.5.5V5h-.5A1.5 1.5 0 0 0 0 6.5v7a1.5 1.5 0 0 0 1 1.415v.335a.75.75 0 0 0 1.5 0V15H4v-1H1.5a.5.5 0 0 1-.5-.5v-7a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 .5.5V7h1v-.5A1.5 1.5 0 0 0 6.5 5H6V1.5a.5.5 0 0 0-.5-.5zM5 5H3V2h2z\"/>\n  <path d=\"M3 7.5a.5.5 0 0 0-1 0v5a.5.5 0 0 0 1 0zM11 6a1.5 1.5 0 0 1 1.5 1.5V8h2A1.5 1.5 0 0 1 16 9.5v5a1.5 1.5 0 0 1-1.5 1.5h-8A1.5 1.5 0 0 1 5 14.5v-5A1.5 1.5 0 0 1 6.5 8h2v-.5A1.5 1.5 0 0 1 10 6zM9.5 7.5V8h2v-.5A.5.5 0 0 0 11 7h-1a.5.5 0 0 0-.5.5M6 9.5v5a.5.5 0 0 0 .5.5H7V9h-.5a.5.5 0 0 0-.5.5m7 5.5V9H8v6zm1.5 0a.5.5 0 0 0 .5-.5v-5a.5.5 0 0 0-.5-.5H14v6z\"/>\n</svg>",
                                        "1PC MAX 15LB 7KG 115LCM" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-luggage\" viewBox=\"0 0 16 16\">\n  <path d=\"M2.5 1a.5.5 0 0 0-.5.5V5h-.5A1.5 1.5 0 0 0 0 6.5v7a1.5 1.5 0 0 0 1 1.415v.335a.75.75 0 0 0 1.5 0V15H4v-1H1.5a.5.5 0 0 1-.5-.5v-7a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 .5.5V7h1v-.5A1.5 1.5 0 0 0 6.5 5H6V1.5a.5.5 0 0 0-.5-.5zM5 5H3V2h2z\"/>\n  <path d=\"M3 7.5a.5.5 0 0 0-1 0v5a.5.5 0 0 0 1 0zM11 6a1.5 1.5 0 0 1 1.5 1.5V8h2A1.5 1.5 0 0 1 16 9.5v5a1.5 1.5 0 0 1-1.5 1.5h-8A1.5 1.5 0 0 1 5 14.5v-5A1.5 1.5 0 0 1 6.5 8h2v-.5A1.5 1.5 0 0 1 10 6zM9.5 7.5V8h2v-.5A.5.5 0 0 0 11 7h-1a.5.5 0 0 0-.5.5M6 9.5v5a.5.5 0 0 0 .5.5H7V9h-.5a.5.5 0 0 0-.5.5m7 5.5V9H8v6zm1.5 0a.5.5 0 0 0 .5-.5v-5a.5.5 0 0 0-.5-.5H14v6z\"/>\n</svg>",
                                        _ => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 320 512\"><path fill=\"currentColor\" d=\"M48 160C48 98.1 98.1 48 160 48S272 98.1 272 160c0 48.2-30.5 89.4-73.3 105.1-29.4 10.8-62.7 37.9-62.7 78.9l0 16c0 13.3 10.7 24 24 24s24-10.7 24-24l0-16c0-12.1 11-26.3 31.3-33.8 61.1-22.5 104.7-81.2 104.7-150.2 0-88.4-71.6-160-160-160S0 71.6 0 160l0 8c0 13.3 10.7 24 24 24s24-10.7 24-24l0-8zM160 512c17.7 0 32-14.3 32-32s-14.3-32-32-32-32 14.3-32 32 14.3 32 32 32z\"/></svg>"
                                    };
                                    _amenity.IconClass = a.Description switch
                                    {
                                        "PRE PAID BAGGAGE" => "<i class=\"bi bi-luggage\"></i>",
                                        "1PC MAX 23KG 158LCM EACH" => "<i class=\"bi bi-luggage\"></i>",
                                        _ => ""
                                    };
                                    _amenity.IsChargeable = a.IsChargeable;
                                    _amenity.IsActive = true;
                                    Amenities.Add(_amenity);
                                    break;

                                case "BRANDED_FARES":
                                    _amenity.Type = AmenityType.BRANDED_FARES;
                                    _amenity.Name = a.Description switch
                                    {
                                        "STATUS CREDIT ACCRUAL" => "Status Credit Accrual",
                                        "STANDARD SEATING" => "Standard Seating",
                                        "POINTS ACCRUAL" => "Points Accrual",
                                        "REFUNDABLE TICKET" => "Refundable Ticket",
                                        "MILEAGE ACCRUAL" => "Mileage Accrual",
                                        "CHANGEABLE TICKET" => "Changeable Ticket",
                                        _ => a.Description!
                                    };
                                    _amenity.SvgPath = a.Description switch
                                    {
                                        "STATUS CREDIT ACCRUAL" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-stars\" viewBox=\"0 0 16 16\">  <path d=\"M7.657 6.247c.11-.33.576-.33.686 0l.645 1.937a2.89 2.89 0 0 0 1.829 1.828l1.936.645c.33.11.33.576 0 .686l-1.937.645a2.89 2.89 0 0 0-1.828 1.829l-.645 1.936a.361.361 0 0 1-.686 0l-.645-1.937a2.89 2.89 0 0 0-1.828-1.828l-1.937-.645a.361.361 0 0 1 0-.686l1.937-.645a2.89 2.89 0 0 0 1.828-1.828zM3.794 1.148a.217.217 0 0 1 .412 0l.387 1.162c.173.518.579.924 1.097 1.097l1.162.387a.217.217 0 0 1 0 .412l-1.162.387A1.73 1.73 0 0 0 4.593 5.69l-.387 1.162a.217.217 0 0 1-.412 0L3.407 5.69A1.73 1.73 0 0 0 2.31 4.593l-1.162-.387a.217.217 0 0 1 0-.412l1.162-.387A1.73 1.73 0 0 0 3.407 2.31zM10.863.099a.145.145 0 0 1 .274 0l.258.774c.115.346.386.617.732.732l.774.258a.145.145 0 0 1 0 .274l-.774.258a1.16 1.16 0 0 0-.732.732l-.258.774a.145.145 0 0 1-.274 0l-.258-.774a1.16 1.16 0 0 0-.732-.732L9.1 2.137a.145.145 0 0 1 0-.274l.774-.258c.346-.115.617-.386.732-.732z\"/></svg>",
                                        "STANDARD SEATING" => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 384 512\"><path fill=\"currentColor\" d=\"M256 48c8.8 0 16 7.2 16 16l0 192-160 0 0-192c0-8.8 7.2-16 16-16l128 0zM64 64l0 192-16 0c-26.5 0-48 21.5-48 48l0 48c0 20.9 13.4 38.7 32 45.3L32 488c0 13.3 10.7 24 24 24s24-10.7 24-24l0-88 224 0 0 88c0 13.3 10.7 24 24 24s24-10.7 24-24l0-90.7c18.6-6.6 32-24.4 32-45.3l0-48c0-26.5-21.5-48-48-48l-16 0 0-192c0-35.3-28.7-64-64-64L128 0C92.7 0 64 28.7 64 64zM328 352l-280 0 0-48 288 0 0 48-8 0z\"/></svg>",
                                        "POINTS ACCRUAL" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-award\" viewBox=\"0 0 16 16\">  <path d=\"M9.669.864 8 0 6.331.864l-1.858.282-.842 1.68-1.337 1.32L2.6 6l-.306 1.854 1.337 1.32.842 1.68 1.858.282L8 12l1.669-.864 1.858-.282.842-1.68 1.337-1.32L13.4 6l.306-1.854-1.337-1.32-.842-1.68zm1.196 1.193.684 1.365 1.086 1.072L12.387 6l.248 1.506-1.086 1.072-.684 1.365-1.51.229L8 10.874l-1.355-.702-1.51-.229-.684-1.365-1.086-1.072L3.614 6l-.25-1.506 1.087-1.072.684-1.365 1.51-.229L8 1.126l1.356.702z\"/>  <path d=\"M4 11.794V16l4-1 4 1v-4.206l-2.018.306L8 13.126 6.018 12.1z\"/></svg>",
                                        "REFUNDABLE TICKET" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-ticket-detailed\" viewBox=\"0 0 16 16\">  <path d=\"M4 5.5a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7a.5.5 0 0 1-.5-.5m0 5a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7a.5.5 0 0 1-.5-.5M5 7a1 1 0 0 0 0 2h6a1 1 0 1 0 0-2z\"/>  <path d=\"M0 4.5A1.5 1.5 0 0 1 1.5 3h13A1.5 1.5 0 0 1 16 4.5V6a.5.5 0 0 1-.5.5 1.5 1.5 0 0 0 0 3 .5.5 0 0 1 .5.5v1.5a1.5 1.5 0 0 1-1.5 1.5h-13A1.5 1.5 0 0 1 0 11.5V10a.5.5 0 0 1 .5-.5 1.5 1.5 0 1 0 0-3A.5.5 0 0 1 0 6zM1.5 4a.5.5 0 0 0-.5.5v1.05a2.5 2.5 0 0 1 0 4.9v1.05a.5.5 0 0 0 .5.5h13a.5.5 0 0 0 .5-.5v-1.05a2.5 2.5 0 0 1 0-4.9V4.5a.5.5 0 0 0-.5-.5z\"/></svg>",
                                        "MILEAGE ACCRUAL" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-award\" viewBox=\"0 0 16 16\">  <path d=\"M9.669.864 8 0 6.331.864l-1.858.282-.842 1.68-1.337 1.32L2.6 6l-.306 1.854 1.337 1.32.842 1.68 1.858.282L8 12l1.669-.864 1.858-.282.842-1.68 1.337-1.32L13.4 6l.306-1.854-1.337-1.32-.842-1.68zm1.196 1.193.684 1.365 1.086 1.072L12.387 6l.248 1.506-1.086 1.072-.684 1.365-1.51.229L8 10.874l-1.355-.702-1.51-.229-.684-1.365-1.086-1.072L3.614 6l-.25-1.506 1.087-1.072.684-1.365 1.51-.229L8 1.126l1.356.702z\"/>  <path d=\"M4 11.794V16l4-1 4 1v-4.206l-2.018.306L8 13.126 6.018 12.1z\"/></svg>",
                                        "CHANGEABLE TICKET" => "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"16\" height=\"16\" fill=\"currentColor\" class=\"bi bi-ticket-detailed\" viewBox=\"0 0 16 16\">  <path d=\"M4 5.5a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7a.5.5 0 0 1-.5-.5m0 5a.5.5 0 0 1 .5-.5h7a.5.5 0 0 1 0 1h-7a.5.5 0 0 1-.5-.5M5 7a1 1 0 0 0 0 2h6a1 1 0 1 0 0-2z\"/>  <path d=\"M0 4.5A1.5 1.5 0 0 1 1.5 3h13A1.5 1.5 0 0 1 16 4.5V6a.5.5 0 0 1-.5.5 1.5 1.5 0 0 0 0 3 .5.5 0 0 1 .5.5v1.5a1.5 1.5 0 0 1-1.5 1.5h-13A1.5 1.5 0 0 1 0 11.5V10a.5.5 0 0 1 .5-.5 1.5 1.5 0 1 0 0-3A.5.5 0 0 1 0 6zM1.5 4a.5.5 0 0 0-.5.5v1.05a2.5 2.5 0 0 1 0 4.9v1.05a.5.5 0 0 0 .5.5h13a.5.5 0 0 0 .5-.5v-1.05a2.5 2.5 0 0 1 0-4.9V4.5a.5.5 0 0 0-.5-.5z\"/></svg>",
                                        _ => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 320 512\"><path fill=\"currentColor\" d=\"M48 160C48 98.1 98.1 48 160 48S272 98.1 272 160c0 48.2-30.5 89.4-73.3 105.1-29.4 10.8-62.7 37.9-62.7 78.9l0 16c0 13.3 10.7 24 24 24s24-10.7 24-24l0-16c0-12.1 11-26.3 31.3-33.8 61.1-22.5 104.7-81.2 104.7-150.2 0-88.4-71.6-160-160-160S0 71.6 0 160l0 8c0 13.3 10.7 24 24 24s24-10.7 24-24l0-8zM160 512c17.7 0 32-14.3 32-32s-14.3-32-32-32-32 14.3-32 32 14.3 32 32 32z\"/></svg>"
                                    };
                                    _amenity.IconClass = a.Description switch
                                    {
                                        "STATUS CREDIT ACCRUAL" => "",
                                        "STANDARD SEATING" => "",
                                        "POINTS ACCRUAL" => "",
                                        "REFUNDABLE TICKET" => "",
                                        "MILEAGE ACCRUAL" => "",
                                        _ => ""
                                    };
                                    _amenity.IsChargeable = a.IsChargeable;
                                    _amenity.IsActive = true;
                                    Amenities.Add(_amenity);
                                    break;

                                case "MEAL":
                                    _amenity.Type = AmenityType.MEAL;
                                    _amenity.Name = a.Description switch
                                    {
                                        "COMPLIMENTARY BEVERAGES" => "Complimentary Beverages",
                                        "MEAL OR SNACK" => "Meal or Snack",
                                        "SPECIAL MEAL" => "Special Meal",
                                        _ => a.Description!
                                    };
                                    _amenity.SvgPath = _amenity.Name switch
                                    {
                                        "Complimentary Beverages" => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 576 512\"><path fill=\"currentColor\" d=\"M112 80l288 0 0 208c0 26.5-21.5 48-48 48l-192 0c-26.5 0-48-21.5-48-48l0-208zM448 224l0-144 8 0c39.8 0 72 32.2 72 72s-32.2 72-72 72l-8 0zm0 64l0-16 8 0c66.3 0 120-53.7 120-120S522.3 32 456 32L96 32C78.3 32 64 46.3 64 64l0 224c0 53 43 96 96 96l192 0c53 0 96-43 96-96zM56 464c-13.3 0-24 10.7-24 24s10.7 24 24 24l400 0c13.3 0 24-10.7 24-24s-10.7-24-24-24L56 464z\"/></svg>",
                                        "Meal or Snack" => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 576 512\"><path fill=\"currentColor\" d=\"M264.6-16C239.2-16 217 1.1 210.5 25.6L191.7 96 64 96c-13.3 0-24 10.7-24 24s10.7 24 24 24L92.2 468.2C94.3 493 115.1 512 140 512l71.1 0c-2-8.1-3.1-16.6-3.1-25.3l0-22.7-68 0-27.8-320 223.6 0-3.9 45.4c14.7-5.2 31.1-9.1 49.2-11.4l3-34c13.3 0 24-10.7 24-24s-10.7-24-24-24l-142.6 0 15.5-58.1c.9-3.5 4.1-5.9 7.7-5.9L296 32c13.3 0 24-10.7 24-24s-10.7-24-24-24l-31.4 0zM304 338.5c0-8.1 1.3-10.9 1.6-11.3 9.9-17.3 38.4-55.3 110.4-55.4 72 .1 100.5 38.1 110.4 55.4 .3 .4 1.6 3.2 1.6 11.3l0 29.3 48 0 0-29.3c0-12.3-1.8-24.6-7.9-35.2-15.8-27.5-58-79.4-152.1-79.5-94.1 .1-136.3 52-152.1 79.5-6.1 10.7-7.9 22.9-7.9 35.2l0 29.3 48 0 0-29.3zM256 486.7c0 31.6 25.6 57.1 57.1 57.1l205.7 0c31.6 0 57.1-25.6 57.1-57.1l0-6.9-48 0 0 6.9c0 5-4.1 9.1-9.1 9.1l-205.7 0c-5 0-9.1-4.1-9.1-9.1l0-6.9-48 0 0 6.9zm24-86.9c-13.3 0-24 10.7-24 24s10.7 24 24 24l272 0c13.3 0 24-10.7 24-24s-10.7-24-24-24l-272 0zM432 304a16 16 0 1 0 -32 0 16 16 0 1 0 32 0zm-80 48a16 16 0 1 0 0-32 16 16 0 1 0 0 32zm144-16a16 16 0 1 0 -32 0 16 16 0 1 0 32 0z\"/></svg>",
                                        "Special Meal" => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 512 512\"><path fill=\"currentColor\" d=\"M96 78L13.1 93.6c-7.6 1.4-13.1 8-13.1 15.7 0 9.8 8.8 17.3 18.5 15.8l77.5-12.1 0 28-80.3 2.5C7 143.8 0 151 0 159.7 0 168.7 7.3 176 16.2 176l79.8 0 0 48-48 0c-26.5 0-48 21.5-48 48 0 90.8 54.1 169 131.7 204.2 8.1 21 28.4 35.8 52.3 35.8l144 0c23.8 0 44.2-14.9 52.3-35.8 77.7-35.2 131.7-113.4 131.7-204.2 0-26.5-21.5-48-48-48l-224 0 0-184c0-13.3-10.7-24-24-24s-24 10.7-24 24l0 184-48 0 0-168c0-13.3-10.7-24-24-24S96 42.7 96 56l0 22zm192 57l0 41 200.4 0c13 0 23.6-10.6 23.6-23.6 0-13.3-11-24-24.4-23.6L288 135zm0-93l0 41 204.3-31.9c11.3-1.8 19.7-11.5 19.7-23 0-14.6-13.3-25.6-27.6-22.9L288 42zM151.5 432.5C90.4 404.8 48 343.3 48 272l416 0c0 71.3-42.4 132.8-103.5 160.5-11.5 5.2-20.4 14.7-25 26.4-1.2 3.1-4.2 5.1-7.5 5.1l-144 0c-3.3 0-6.3-2-7.5-5.1-4.5-11.7-13.5-21.2-25-26.4z\"/></svg>",
                                        _ => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 320 512\"><path fill=\"currentColor\" d=\"M48 160C48 98.1 98.1 48 160 48S272 98.1 272 160c0 48.2-30.5 89.4-73.3 105.1-29.4 10.8-62.7 37.9-62.7 78.9l0 16c0 13.3 10.7 24 24 24s24-10.7 24-24l0-16c0-12.1 11-26.3 31.3-33.8 61.1-22.5 104.7-81.2 104.7-150.2 0-88.4-71.6-160-160-160S0 71.6 0 160l0 8c0 13.3 10.7 24 24 24s24-10.7 24-24l0-8zM160 512c17.7 0 32-14.3 32-32s-14.3-32-32-32-32 14.3-32 32 14.3 32 32 32z\"/></svg>"
                                    };
                                    _amenity.IconClass = _amenity.Name switch
                                    {
                                        "Complimentary Beverages" => "",
                                        "Meal or Snack" => "",
                                        "Special Meal" => "",
                                        _ => ""
                                    };
                                    _amenity.IsChargeable = a.IsChargeable;
                                    _amenity.IsActive = true;
                                    Amenities.Add(_amenity);
                                    break;

                                case "TRAVEL_SERVICES":
                                    _amenity.Type = AmenityType.TRAVEL_SERVICES;
                                    _amenity.Name = a.Description switch
                                    {
                                        "DOMESTIC NAME CHANGE FEE" => "Domestic Name Change Fee",
                                        _ => a.Description!
                                    };
                                    _amenity.SvgPath = a.Description switch
                                    {
                                        "DOMESTIC NAME CHANGE FEE" => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 640 512\"><path fill=\"currentColor\" d=\"M240.1 48l-112 0c-8.8 0-16 7.2-16 16l0 384c0 8.8 7.2 16 16 16l155.8 0-9.6 48-146.2 0c-35.3 0-64-28.7-64-64l0-384c0-35.3 28.7-64 64-64L261.6 0c17 0 33.3 6.7 45.3 18.7L429.3 141.3c12 12 18.7 28.3 18.7 45.3l0 81.5-48 48 0-108-88 0c-39.8 0-72-32.2-72-72l0-88zM380.2 160l-92.1-92.1 0 68.1c0 13.3 10.7 24 24 24l68.1 0zM332.3 466.9c2.5-12.4 8.6-23.8 17.5-32.7l118.9-118.9 80 80-118.9 118.9c-8.9 8.9-20.3 15-32.7 17.5l-59.6 11.9c-.9 .2-1.9 .3-2.9 .3-8 0-14.6-6.5-14.6-14.6 0-1 .1-1.9 .3-2.9l11.9-59.6zm267.8-123l-28.8 28.8-80-80 28.8-28.8c22.1-22.1 57.9-22.1 80 0s22.1 57.9 0 80z\"/></svg>",
                                        _ => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 320 512\"><path fill=\"currentColor\" d=\"M48 160C48 98.1 98.1 48 160 48S272 98.1 272 160c0 48.2-30.5 89.4-73.3 105.1-29.4 10.8-62.7 37.9-62.7 78.9l0 16c0 13.3 10.7 24 24 24s24-10.7 24-24l0-16c0-12.1 11-26.3 31.3-33.8 61.1-22.5 104.7-81.2 104.7-150.2 0-88.4-71.6-160-160-160S0 71.6 0 160l0 8c0 13.3 10.7 24 24 24s24-10.7 24-24l0-8zM160 512c17.7 0 32-14.3 32-32s-14.3-32-32-32-32 14.3-32 32 14.3 32 32 32z\"/></svg>"
                                    };
                                    _amenity.IconClass = a.Description switch
                                    {
                                        "DOMESTIC NAME CHANGE FEE" => "",
                                        _ => ""
                                    };
                                    _amenity.IsChargeable = a.IsChargeable;
                                    _amenity.IsActive = true;
                                    Amenities.Add(_amenity);
                                    break;

                                case "PRE_RESERVED_SEAT":
                                    _amenity.Type = AmenityType.PRE_RESERVED_SEAT;
                                    _amenity.Name = a.Description switch
                                    {
                                        "SEAT ASSIGNMENT" => "Seat Assignment",
                                        _ => a.Description!
                                    };
                                    _amenity.SvgPath = a.Description switch
                                    {
                                        "SEAT ASSIGNMENT" => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 640 512\"><path fill=\"currentColor\" d=\"M240.1 48l-112 0c-8.8 0-16 7.2-16 16l0 384c0 8.8 7.2 16 16 16l155.8 0-9.6 48-146.2 0c-35.3 0-64-28.7-64-64l0-384c0-35.3 28.7-64 64-64L261.6 0c17 0 33.3 6.7 45.3 18.7L429.3 141.3c12 12 18.7 28.3 18.7 45.3l0 81.5-48 48 0-108-88 0c-39.8 0-72-32.2-72-72l0-88zM380.2 160l-92.1-92.1 0 68.1c0 13.3 10.7 24 24 24l68.1 0zM332.3 466.9c2.5-12.4 8.6-23.8 17.5-32.7l118.9-118.9 80 80-118.9 118.9c-8.9 8.9-20.3 15-32.7 17.5l-59.6 11.9c-.9 .2-1.9 .3-2.9 .3-8 0-14.6-6.5-14.6-14.6 0-1 .1-1.9 .3-2.9l11.9-59.6zm267.8-123l-28.8 28.8-80-80 28.8-28.8c22.1-22.1 57.9-22.1 80 0s22.1 57.9 0 80z\"/></svg>",
                                        _ => "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 320 512\"><path fill=\"currentColor\" d=\"M48 160C48 98.1 98.1 48 160 48S272 98.1 272 160c0 48.2-30.5 89.4-73.3 105.1-29.4 10.8-62.7 37.9-62.7 78.9l0 16c0 13.3 10.7 24 24 24s24-10.7 24-24l0-16c0-12.1 11-26.3 31.3-33.8 61.1-22.5 104.7-81.2 104.7-150.2 0-88.4-71.6-160-160-160S0 71.6 0 160l0 8c0 13.3 10.7 24 24 24s24-10.7 24-24l0-8zM160 512c17.7 0 32-14.3 32-32s-14.3-32-32-32-32 14.3-32 32 14.3 32 32 32z\"/></svg>"

                                    };
                                    _amenity.IconClass = a.Description switch
                                    {
                                        "SEAT ASSIGNMENT" => "",
                                        _ => ""
                                    };
                                    _amenity.IsChargeable = a.IsChargeable;
                                    _amenity.IsActive = true;
                                    Amenities.Add(_amenity);
                                    break;

                                default:
                                    await _log.WarningAsync(
                                        evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_OFFER_AMENITY_UNKNOWN",
                                        cat: SysLogCatType.App,
                                        act: SysLogActionType.Read,
                                        message: $"Unknown amenity type encountered when mapping flight offer amenity for TravelQuote '{travelQuoteId}': AmenityType='{a.AmenityType}', Description='{a.Description}'");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // no amenities found for segment
                        await _log.WarningAsync(
                            evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_OFFER_AMENITY_MISSING",
                            cat: SysLogCatType.App,
                            act: SysLogActionType.Read,
                            message: $"No amenities found when mapping flight offer amenity for TravelQuote '{travelQuoteId}': SegmentId='{segmentId}'");
                    }

                    // add the amenities to the flight leg
                    fLeg.Amenities = Amenities;

                    fLeg.Layover = i < segs.Count - 1
                        ? new Layover
                        {
                            Airport = seg.Arrival!.IATACode!,
                            Minutes = segs[i + 1].Departure!.At != null && seg.Arrival!.At != null
                                ? (int)(DateTime.Parse(segs[i + 1]?.Departure?.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
                                    - DateTime.Parse(seg.Arrival.At, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)).TotalMinutes
                                : 0,
                        }
                        : null;

                    // add the flight leg to the list of Legs
                    legs.Add(fLeg);
                }
            }

            ///// THIS ONE IS CORRECT ONLY /////
            FlightViewOption flightViewOption = new FlightViewOption
            {
                Origin = quote?.OriginIataCode!,
                Destination = quote?.DestinationIataCode!,

                DepartTime = DateTime.Parse(flightOffer?.Itineraries?.First().Segments?.First().Departure!.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                ArriveTime = DateTime.Parse(flightOffer?.Itineraries?.First().Segments?.Last().Arrival!.At!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),

                Price = CalcTotalFromOrgFeesMarkupDto(costBaseAmount, feeType, markupPercentage, markupAmount),
                Currency = quote?.Currency!,
                CurrencySymbol = currencySymbol,

                Cabins = _cabins.Distinct(StringComparer.OrdinalIgnoreCase).ToList(),

                Stops = stops,

                Amenities = legs.SelectMany(l => l.Amenities)
                    .DistinctBy(a => (a.Type, a.Name, a.IsChargeable, Icon: a.IconClass ?? "", Svg: a.SvgPath ?? ""))
                    .ToList(),

                Legs = legs,

                BaggageText = "1×23kg",
                ChangePolicy = "No",
                RefundPolicy = "No",
                SeatPolicy = "Auto-assign",

                IsOpen = true,

                AmadeusFlightOffer = flightOffer, // raw offer data

                // meta
                QuoteId = travelQuoteId,
            };

            flightViewOptions.Add(flightViewOption);

            await _log.InformationAsync(
                evt: "TRAVEL_QUOTE_GET_FLIGHT_RESULTS_OFFER",
                cat: SysLogCatType.App,
                act: SysLogActionType.Read,
                message: $"Flight offer found for TravelQuote '{travelQuoteId}': OfferId='{flightOffer.Id}', Price='{flightOffer.Price.Total} {flightOffer.Price.Currency}'",
                ent: nameof(TravelQuote),
                entId: travelQuoteId);
        }
        // end foreach flight offer

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
