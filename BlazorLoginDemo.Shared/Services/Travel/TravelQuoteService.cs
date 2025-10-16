// Services/Travel/TravelQuoteService.cs
using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.Kernel.Travel;
using BlazorLoginDemo.Shared.Models.Policies;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Travel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorLoginDemo.Shared.Services.Travel;

internal sealed class TravelQuoteService : ITravelQuoteService
{
    private readonly ApplicationDbContext _db;
    private readonly IAdminOrgServiceUnified _orgSvc;
    private readonly IAdminUserServiceUnified _userSvc;
    private readonly ILogger<TravelQuoteService> _log;

    public TravelQuoteService(
        ApplicationDbContext db,
        IAdminOrgServiceUnified orgSvc,
        IAdminUserServiceUnified userSvc,
        ILogger<TravelQuoteService> log)
    {
        _db = db;
        _orgSvc = orgSvc;
        _userSvc = userSvc;
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
        try
        {
            var quote = await TranslateDtoAsync(dto, ct);

            if (quote.Travellers.Count == 0)
            {
                return (false, "No valid travellers with Travel Policy assigned to generate a quote. Assign users a Travel Policy or assign the organization a Default Travel Policy to generate a quote.", null);
            }

            _db.TravelQuotes.Add(quote);
            await _db.SaveChangesAsync(ct);
            return (true, null, quote.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CreateFromDtoAsync failed");
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
            throw new ArgumentException($"Invalid QuoteType '{dto.QuoteType}'.");

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

        if (dto?.TravellerUserIds is not null)
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
        _log.LogError("Quote DTO translation: {TravellerCount} travellers, {DistinctPolicyCount} distinct non-null policies, {ExcludedUserCount} users excluded due to missing policy.",
            dto?.TravellerUserIds?.Count() ?? 0,
            distinctPolicyIds.Count,
            excludedUserIds.Count);
        foreach (string e in excludedUserIds)
            _log.LogError(" - Excluded user ID: {UserId}", e);

        // obtain all travel policies referenced by travellers
        // Build pL with only currently-effective policies (UTC checks, inclusive bounds)
        if (distinctPolicyIds.Count > 1)
            _log.LogWarning("Quote has travellers with multiple distinct travel policies: {PolicyIds}", string.Join(", ", distinctPolicyIds));

        var pL = new List<TravelPolicy>();
        var nowUtc = DateTime.UtcNow;

        foreach (var pid in distinctPolicyIds)
        {
            var policy = await _db.TravelPolicies.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == pid, ct);

            if (policy is null)
            {
                _log.LogWarning("Travel policy '{PolicyId}' referenced by travellers not found in DB.", pid);
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
            bool expiresOk   = !exp.HasValue || nowUtc <= exp.Value;

            if (effectiveOk && expiresOk)
            {
                pL.Add(policy);
            }
            else
            {
                if (!effectiveOk)
                    _log.LogWarning("Travel policy '{PolicyId}' not yet effective. EffectiveFromUtc={EffectiveFromUtc:o}", pid, eff);

                if (!expiresOk)
                    _log.LogWarning("Travel policy '{PolicyId}' expired. ExpiresOnUtc={ExpiresOnUtc:o}", pid, exp);
            }
        }

        if (pL.Count == 0)
            _log.LogError("No travellers with valid/effective travel policies found for quote.");

        return q;
    }
}
