// Services/Travel/TravelQuoteService.cs
using System.Globalization;
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Models.Search;
using BlazorLoginDemo.Shared.Models.Static;
using BlazorLoginDemo.Shared.Models.Static.SysVar;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using BlazorLoginDemo.Shared.Services.Interfaces.Kernel;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Policy;
using BlazorLoginDemo.Shared.Services.Interfaces.Travel;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Travel;

internal sealed class TravelQuoteService : ITravelQuoteService
{
    private readonly ApplicationDbContext _db;
    private readonly IAdminOrgServiceUnified _orgSvc;
    private readonly IAdminUserServiceUnified _userSvc;
    private readonly ITravelPolicyService _travelPolicySvc;
    private readonly IAirportInfoService _airportInfoSvc;
    private readonly IAirlineService _airlineSvc;
    private readonly ILoggerService _log;

    public TravelQuoteService(
        ApplicationDbContext db,
        IAdminOrgServiceUnified orgSvc,
        IAdminUserServiceUnified userSvc,
        ITravelPolicyService travelPolicySvc,
        IAirportInfoService airportInfoSvc,
        IAirlineService airlineSvc,
        ILoggerService log)
    {
        _db = db;
        _orgSvc = orgSvc;
        _userSvc = userSvc;
        _travelPolicySvc = travelPolicySvc;
        _airportInfoSvc = airportInfoSvc;
        _airlineSvc = airlineSvc;
        _log = log;
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

    // ---------------- UI HELPERS ----------------
    // this can only be called via the API layer where the process is able to consume resources
    public async Task<FlightSearchPageConfig> GenerateFlightSearchUIOptionsAsync(string travelQuoteId, CancellationToken ct = default)
    {
        var quote = await GetByIdAsync(travelQuoteId, ct)
            ?? throw new InvalidOperationException($"TravelQuote '{travelQuoteId}' not found.");

        // Placeholder: actual implementation would generate UI options based on quote details

        var orgName = quote.Organization?.Name ?? "Unknown Org";

        // Retrieve TravelPolicy associated with the quote
        TravelPolicy? travelPolicy = null;

        var policyType = quote.PolicyType;
        switch (policyType)
        {
            case TravelQuotePolicyType.OrgDefault:
            case TravelQuotePolicyType.UserDefined:
                travelPolicy = await _db.TravelPolicies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == quote.TravelPolicyId, ct);
                break;

            case TravelQuotePolicyType.Ephemeral:
                var ephemeralPolicy = await _db.EphemeralTravelPolicies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == quote.TravelPolicyId, ct);

                // Convert EphemeralTravelPolicy to TravelPolicy since they have the same structure
                if (ephemeralPolicy != null)
                {
                    travelPolicy = ConvertEphemeralToTravelPolicy(ephemeralPolicy);
                    await _log.InformationAsync(
                        evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_EPHEMERAL_CONVERTED",
                        cat: SysLogCatType.Workflow,
                        act: SysLogActionType.Step,
                        message: $"Converted EphemeralTravelPolicy '{ephemeralPolicy.Id}' to TravelPolicy for TravelQuote '{travelQuoteId}'",
                        ent: nameof(TravelQuote),
                        entId: travelQuoteId);
                }
                break;

            default:
                await _log.WarningAsync(
                    evt: "TRAVEL_QUOTE_GENERATE_UI_OPTIONS_UNKNOWN_POLICY_TYPE",
                    cat: SysLogCatType.Workflow,
                    act: SysLogActionType.Validate,
                    message: $"Unknown TravelQuotePolicyType '{policyType}' for TravelQuote '{travelQuoteId}'",
                    ent: nameof(TravelQuote),
                    entId: travelQuoteId,
                    note: "unknown_policy_type");
                break;
        }

        // policy name
        var policyName = travelPolicy?.PolicyName ?? "Unknown Policy";

        // we have the travel policy now, so count the users to be processed
        var adults = quote.Travellers.Count;

        // earliest departing date
        DateTime earliestDepartureDate = DateTime.UtcNow.AddDays(0); // default to 0 days from now
        if (travelPolicy?.DefaultCalendarDaysInAdvanceForFlightBooking is int daysInAdvance && daysInAdvance > 0)
        {
            earliestDepartureDate = DateTime.UtcNow.AddDays(daysInAdvance);
        }

        // earliest flight preference time
        string? earliestDepartureTime = null;
        string? latestDepartureTime = null;
        bool _hasFixedTimes = false;

        if (!string.IsNullOrWhiteSpace(travelPolicy?.FlightBookingTimeAvailableFrom) &&
            DateTime.TryParseExact(travelPolicy?.FlightBookingTimeAvailableFrom, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t1))
        {
            earliestDepartureTime = t1.ToString("HH:mm", CultureInfo.InvariantCulture);
            _hasFixedTimes = true;
        }
        if (!string.IsNullOrWhiteSpace(travelPolicy?.FlightBookingTimeAvailableTo) &&
            DateTime.TryParseExact(travelPolicy?.FlightBookingTimeAvailableTo, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t2))
        {
            latestDepartureTime = t2.ToString("HH:mm", CultureInfo.InvariantCulture);
            _hasFixedTimes = true;
        }


        // await _db.TravelPolicies
        //     .AsNoTracking()
        //     .FirstOrDefaultAsync(p => p.Id == quote.TravelPolicyId, ct);

        // find allowed countries, this is used for airport codes and airlines (virtual)
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

        // create to/from lists based on allowed countries
        List<Iso3166_Alpha2> allowedIso3166_Alpha2 =
            allowedCountries
                .Select(c => c.IsoCode)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => Enum.TryParse<Iso3166_Alpha2>(s.Trim(), ignoreCase: true, out var e) ? e : (Iso3166_Alpha2?)null)
                .Where(e => e.HasValue)
                .Select(e => e!.Value)
                .Distinct()
                .ToList();

        List<BookingAirport> originsDestinations =
            (await _airportInfoSvc.SearchMultiAsync(
                types: new List<AirportType> { AirportType.MediumAirport, AirportType.LargeAirport },
                countries: allowedIso3166_Alpha2,
                hasIata: true,
                hasMunicipality: true,
                ct: ct
            ))
            .Select(a => new BookingAirport(
                Code: a.IataCode!,
                Name: a.Name,
                City: a.Municipality,
                Country: a.IsoCountry.ToString()))
            .ToList();

        // allowed airlines (virtual)
        // 1) Pick codes once with normalization
        string[] excludedAirlines = NormalizeAirlineCodes(travelPolicy?.ExcludedAirlineCodes);
        string[] includedAirlines = NormalizeAirlineCodes(travelPolicy?.IncludedAirlineCodes);

        string[] availableAirlines = excludedAirlines.Length > 0
            ? excludedAirlines
            : includedAirlines.Length > 0
                ? includedAirlines
                : Array.Empty<string>();

        // 2) Resolve to BookingAirline
        var bookingAirlines = new List<BookingAirline>(availableAirlines.Length);

        foreach (var code in availableAirlines)
        {
            // Try IATA if 2 chars, else ICAO if 3 chars
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


        // cabin class
        var map = new Dictionary<string, CabinClass>(StringComparer.OrdinalIgnoreCase)
        {
            ["ECONOMY"] = CabinClass.Economy,
            ["PREMIUM_ECONOMY"] = CabinClass.PremiumEconomy,
            ["BUSINESS"] = CabinClass.Business,
            ["FIRST"] = CabinClass.First
        };

        CabinClass? defaultCabin = null;
        string? defaultCabinLabel = null;
        CabinClass? maxCabin = null;
        string? maxCabinLabel = null;

        var cabinDefault = travelPolicy?.DefaultFlightSeating;
        if (!string.IsNullOrWhiteSpace(cabinDefault) && map.TryGetValue(cabinDefault.Trim(), out var cab1))
        {
            defaultCabin = cab1;
            defaultCabinLabel = CabinLabel(cab1);
        }

        var cabinMax = travelPolicy?.MaxFlightSeating;
        if (!string.IsNullOrWhiteSpace(cabinMax) && map.TryGetValue(cabinMax.Trim(), out var cab2))
        {
            maxCabin = cab2;
            maxCabinLabel = CabinLabel(cab2);
        }

        // coverage type
        var coverageMap = new Dictionary<string, CoverageType>(StringComparer.OrdinalIgnoreCase)
        {
            ["MOST_SEGMENTS"] = CoverageType.MostSegments,
            ["AT_LEAST_ONE_SEGMENT"] = CoverageType.AtLeastOneSegment,
            ["ALL_SEGMENTS"] = CoverageType.AllSegments
        };

        CoverageType? coverageType = null;
        string? coverageTypeLabel = null;
        var coverageStr = travelPolicy?.CabinClassCoverage;
        if (!string.IsNullOrWhiteSpace(coverageStr) && coverageMap.TryGetValue(coverageStr.Trim(), out var cov))
        {
            coverageType = cov;
            coverageTypeLabel = coverageStr.Trim();
        };

        // create the UI options object
        FlightSearchPageConfig config = new FlightSearchPageConfig
        {
            TenantName = orgName,
            PolicyName = policyName,
            TravelQuoteId = travelQuoteId,
            TmcAssignedId = quote.TmcAssignedId,
            EnabledOrigins = originsDestinations,
            EnabledDestinations = originsDestinations,
            AvailableAirlines = bookingAirlines,
            PreferredAirlines = includedAirlines.ToList(),
            DefaultCabin = defaultCabin ?? CabinClass.Economy,
            MaxCabin = maxCabin ?? CabinClass.First,
            DaysInAdvanceBookingRequired = travelPolicy?.DefaultCalendarDaysInAdvanceForFlightBooking,
            HasFixedTimes = _hasFixedTimes,
            FixedDepartEarliest = earliestDepartureTime ?? string.Empty,
            FixedDepartLatest = latestDepartureTime ?? string.Empty,
            SeedDepartDate = earliestDepartureDate,
            Adults = adults
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
            var orgDefaultId = await _orgSvc.GetOrgDefaultTravelPolicyIdAsync(dto.OrganizationId, ct);

            q.PolicyType = string.Equals(pL[0].Id, orgDefaultId, StringComparison.Ordinal)
                ? TravelQuotePolicyType.OrgDefault
                : TravelQuotePolicyType.UserDefined;
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

            q.TravelPolicyId = eTravelPolicy.Id;
            q.PolicyType = TravelQuotePolicyType.Ephemeral;
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
}
