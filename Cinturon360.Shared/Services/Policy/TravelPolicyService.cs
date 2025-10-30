using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Policies;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Services.Interfaces.Policy;
using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Shared.Services.Policy;

public sealed class TravelPolicyService : ITravelPolicyService
{
    private readonly ApplicationDbContext _db;
    private readonly ILoggerService _logger;

    public TravelPolicyService(ApplicationDbContext db, ILoggerService logger)
    {
        _db = db;
        _logger = logger;
    }

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<TravelPolicy> CreateAsync(TravelPolicy policy, CancellationToken ct = default)
    {
        if (policy is null) throw new ArgumentNullException(nameof(policy));
        if (string.IsNullOrWhiteSpace(policy.OrganizationUnifiedId))
            throw new ArgumentException("OrganizationUnifiedId must be provided.", nameof(policy));
        if (string.IsNullOrWhiteSpace(policy.PolicyName))
            throw new ArgumentException("PolicyName must be provided.", nameof(policy));
        if (string.IsNullOrWhiteSpace(policy.DefaultCurrencyCode))
            throw new ArgumentException("DefaultCurrencyCode must be provided.", nameof(policy));

        if (string.IsNullOrWhiteSpace(policy.Id))
            policy.Id = NanoidDotNet.Nanoid.Generate(NanoidDotNet.Nanoid.Alphabets.LettersAndDigits.ToUpper(), 14);

        NormalizePolicyLists(policy);
        NormalizeGeoIds(policy);

        var orgExists = await _db.Organizations.AsNoTracking().AnyAsync(o => o.Id == policy.OrganizationUnifiedId, ct);
        if (!orgExists) throw new InvalidOperationException($"Organization '{policy.OrganizationUnifiedId}' not found.");

        await _logger.InformationAsync(
            evt: "TRAVEL_POLICY_CREATE",
            cat: SysLogCatType.Data,
            act: SysLogActionType.Create,
            message: $"Creating TravelPolicy '{policy.PolicyName}' for Org '{policy.OrganizationUnifiedId}'",
            ent: nameof(TravelPolicy),
            entId: policy.Id);

        static DateTime? EnsureUtc(DateTime? d) =>
            d is null ? null :
            d.Value.Kind == DateTimeKind.Utc ? d :
            DateTime.SpecifyKind(d.Value, DateTimeKind.Utc);

        policy.EffectiveFromUtc = EnsureUtc(policy.EffectiveFromUtc);
        policy.ExpiresOnUtc     = EnsureUtc(policy.ExpiresOnUtc);

        policy.CreatedAtUtc = DateTime.UtcNow;
        policy.LastUpdatedUtc = DateTime.UtcNow;

        await _db.TravelPolicies.AddAsync(policy, ct);

        var org = await _db.Organizations.FirstOrDefaultAsync(
            o => o.Id == policy.OrganizationUnifiedId, ct);
        if (org is null)
            throw new InvalidOperationException($"Organization '{policy.OrganizationUnifiedId}' not found on add.");

        if (string.IsNullOrWhiteSpace(org.DefaultTravelPolicyId))
        {
            org.DefaultTravelPolicyId = policy.Id;
            org.LastUpdatedUtc = DateTime.UtcNow;

            await _logger.InformationAsync(
                evt: "TRAVEL_POLICY_SET_DEFAULT_ON_CREATE",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                message: $"Organization '{org.Id}' default Travel Policy set to '{policy.Id}' on creation.",
                ent: nameof(TravelPolicy),
                entId: policy.Id);

            await _db.SaveChangesAsync(ct);
        }
        else
        {
            _db.Entry(org).State = EntityState.Detached; // "let go"
        }

        await _db.SaveChangesAsync(ct);
        return policy;
    }

    public async Task<TravelPolicy> CreateDefaultAsync(TravelPolicy policy, CancellationToken ct = default)
        => await CreateAsync(policy, ct);

    // -----------------------------
    // READ
    // -----------------------------
    public async Task<TravelPolicy?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.TravelPolicies.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<TravelPolicy>> GetAllAsync(CancellationToken ct = default)
        => await _db.TravelPolicies.AsNoTracking()
            .OrderBy(x => x.OrganizationUnifiedId)
            .ThenBy(x => x.PolicyName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<TravelPolicy>> GetForOrganizationAsync(string organizationId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(organizationId)) return Array.Empty<TravelPolicy>();
        return await _db.TravelPolicies.AsNoTracking()
            .Where(x => x.OrganizationUnifiedId == organizationId)
            .OrderBy(x => x.PolicyName)
            .ToListAsync(ct);
    }

    // -----------------------------
    // UPDATE (replace whole object)
    // -----------------------------
    public async Task<TravelPolicy> UpdateAsync(TravelPolicy policy, CancellationToken ct = default)
    {
        if (policy is null) throw new ArgumentNullException(nameof(policy));
        if (string.IsNullOrWhiteSpace(policy.Id))
            throw new ArgumentException("Id must be provided for update.", nameof(policy));
        if (string.IsNullOrWhiteSpace(policy.OrganizationUnifiedId))
            throw new ArgumentException("OrganizationUnifiedId must be provided.", nameof(policy));
        if (string.IsNullOrWhiteSpace(policy.PolicyName))
            throw new ArgumentException("PolicyName must be provided.", nameof(policy));
        if (string.IsNullOrWhiteSpace(policy.DefaultCurrencyCode))
            throw new ArgumentException("DefaultCurrencyCode must be provided.", nameof(policy));

        var orgExists = await _db.Organizations.AsNoTracking().AnyAsync(o => o.Id == policy.OrganizationUnifiedId, ct);
        if (!orgExists) throw new InvalidOperationException($"Organization '{policy.OrganizationUnifiedId}' not found.");

        NormalizePolicyLists(policy);
        NormalizeGeoIds(policy);

        static DateTime? EnsureUtc(DateTime? d) =>
            d is null ? null :
            d.Value.Kind == DateTimeKind.Utc ? d :
            DateTime.SpecifyKind(d.Value, DateTimeKind.Utc);

        policy.EffectiveFromUtc = EnsureUtc(policy.EffectiveFromUtc);
        policy.ExpiresOnUtc     = EnsureUtc(policy.ExpiresOnUtc);
        policy.LastUpdatedUtc   = DateTime.UtcNow;

        _db.Attach(policy);
        _db.Entry(policy).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return policy;
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _db.TravelPolicies.FindAsync([id], ct);
        if (existing is null) return false;

        _db.TravelPolicies.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // UTIL
    // -----------------------------
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.TravelPolicies.AsNoTracking().AnyAsync(x => x.Id == id, ct);

    public async Task<bool> SetAsDefaultPolicyAsync(string policyId, string organizationId, bool isNew = false, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(policyId))
            throw new ArgumentException("policyId must be provided.", nameof(policyId));
        if (string.IsNullOrWhiteSpace(organizationId))
            throw new ArgumentException("organizationId must be provided.", nameof(organizationId));

        var org = await _db.Organizations.FindAsync([organizationId], ct)
                ?? throw new InvalidOperationException($"Organization '{organizationId}' not found.");

        var current = org.DefaultTravelPolicyId;

        if (string.IsNullOrWhiteSpace(current))
        {
            if (isNew)
            {
                org.DefaultTravelPolicyId = policyId;
                await _db.SaveChangesAsync(ct);
                return true;
            }
            return false;
        }

        if (string.Equals(current, policyId, StringComparison.Ordinal))
            return true;

        if (isNew) return false;

        org.DefaultTravelPolicyId = policyId;
        org.LastUpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> SetPolicyAsDefaultAsync(string policyId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(policyId))
            throw new ArgumentException("policyId must be provided.", nameof(policyId));

        var tp = await _db.TravelPolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == policyId, ct);

        if (tp is null)
        {
            await _logger.ErrorAsync(
                evt: "TRAVEL_POLICY_NOT_FOUND",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new InvalidOperationException($"TravelPolicy '{policyId}' not found."),
                message: $"TravelPolicy '{policyId}' not found.",
                ent: nameof(TravelPolicy),
                entId: policyId,
                note: "not_found");
            return false;
        }

        var org = await _db.Organizations
            .FirstOrDefaultAsync(o => o.Id == tp.OrganizationUnifiedId, ct);

        if (org is null)
        {
            await _logger.ErrorAsync(
                evt: "ORGANIZATION_NOT_FOUND",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                ex: new InvalidOperationException($"Organization '{tp.OrganizationUnifiedId}' from TravelPolicy '{policyId}' not found."),
                message: $"Organization '{tp.OrganizationUnifiedId}' from TravelPolicy '{policyId}' not found.",
                ent: nameof(policyId),
                entId: tp.OrganizationUnifiedId,
                note: "not_found");
            return false;
        }

        if (!string.IsNullOrEmpty(org.DefaultExpensePolicyId))
            await _logger.InformationAsync(
                evt: "TRAVEL_POLICY_UPDATE_DEFAULT",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                message: $"Organization '{org.Id}' default Travel Policy updated from '{tp.OrganizationUnifiedId}' to '{policyId}'.",
                ent: nameof(TravelPolicy),
                entId: policyId);
        else
            await _logger.InformationAsync(
                evt: "TRAVEL_POLICY_SET_DEFAULT",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Update,
                message: $"Organization '{org.Id}' default Travel Policy set to '{policyId}'.",
                ent: nameof(TravelPolicy),
                entId: policyId);

        org.DefaultTravelPolicyId = policyId;
        org.LastUpdatedUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // RESOLUTION (countries only)
    // -----------------------------
    public async Task<IReadOnlyList<Country>> ResolveAllowedCountriesAsync(string policyId, CancellationToken ct = default)
    {
        // Load policy
        var tp = await _db.TravelPolicies.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == policyId, ct)
            ?? throw new InvalidOperationException($"TravelPolicy '{policyId}' not found");

        // 1) Start with explicit country IDs from policy
        var bag = new List<int>(tp.CountryIds);

        // 2) Add countries for explicit continents
        if (tp.ContinentIds.Length > 0)
        {
            var contIds = tp.ContinentIds; // capture to local for translation
            var contCountryIds = await _db.Countries.AsNoTracking()
                .Where(c => c.ContinentId.HasValue && contIds.Contains(c.ContinentId.Value))
                .Select(c => c.Id)
                .ToListAsync(ct);
            bag.AddRange(contCountryIds);
        }

        // 3) Add countries for regions → continents → countries
        if (tp.RegionIds.Length > 0)
        {
            var regionIds = tp.RegionIds;
            var regionContinentIds = await _db.Continents.AsNoTracking()
                .Where(x => x.RegionId.HasValue && regionIds.Contains(x.RegionId.Value))
                .Select(x => x.Id)
                .ToListAsync(ct);

            if (regionContinentIds.Count > 0)
            {
                var regContIds = regionContinentIds;
                var regCountryIds = await _db.Countries.AsNoTracking()
                    .Where(c => c.ContinentId.HasValue && regContIds.Contains(c.ContinentId.Value))
                    .Select(c => c.Id)
                    .ToListAsync(ct);
                bag.AddRange(regCountryIds);
            }
        }

        // 4) Remove disabled, then dedupe
        if (tp.DisabledCountryIds.Length > 0)
        {
            var disabled = tp.DisabledCountryIds;
            bag.RemoveAll(id => disabled.Contains(id));
        }
        var distinctIds = bag.Distinct().ToArray();

        if (distinctIds.Length == 0)
            return Array.Empty<Country>();

        // 5) Return countries sorted by name
        return await _db.Countries.AsNoTracking()
            .Where(c => distinctIds.Contains(c.Id))
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
    }

    // -----------------------------
    // Helpers
    // -----------------------------
    private static void NormalizePolicyLists(TravelPolicy policy)
    {
        static string[] Clean(string[]? arr) =>
            (arr ?? Array.Empty<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim().ToUpperInvariant())
            .Distinct()
            .ToArray();

        static (string[] inc, string[] exc) CleanIncludeExclude(string[]? inc, string[]? exc)
        {
            var included = Clean(inc);
            var excluded = Clean(exc).Except(included).ToArray();
            return (included, excluded);
        }

        (policy.IncludedAirlineCodes, policy.ExcludedAirlineCodes) =
            CleanIncludeExclude(policy.IncludedAirlineCodes, policy.ExcludedAirlineCodes);

        (policy.IncludedHotelChains, policy.ExcludedHotelChains) =
            CleanIncludeExclude(policy.IncludedHotelChains, policy.ExcludedHotelChains);

        (policy.IncludedTaxiVendors, policy.ExcludedTaxiVendors) =
            CleanIncludeExclude(policy.IncludedTaxiVendors, policy.ExcludedTaxiVendors);

        (policy.IncludedRailOperators, policy.ExcludedRailOperators) =
            CleanIncludeExclude(policy.IncludedRailOperators, policy.ExcludedRailOperators);

        policy.AllowedCarHireClasses = Clean(policy.AllowedCarHireClasses);
        (policy.IncludedCarHireVendors, policy.ExcludedCarHireVendors) =
            CleanIncludeExclude(policy.IncludedCarHireVendors, policy.ExcludedCarHireVendors);

        (policy.IncludedBusOperators, policy.ExcludedBusOperators) =
            CleanIncludeExclude(policy.IncludedBusOperators, policy.ExcludedBusOperators);

        (policy.IncludedSimVendors, policy.ExcludedSimVendors) =
            CleanIncludeExclude(policy.IncludedSimVendors, policy.ExcludedSimVendors);

        (policy.IncludedActivityProviders, policy.ExcludedActivityProviders) =
            CleanIncludeExclude(policy.IncludedActivityProviders, policy.ExcludedActivityProviders);
    }

    private static void NormalizeGeoIds(TravelPolicy policy)
    {
        static int[] Clean(int[]? ids) =>
            (ids ?? Array.Empty<int>()).Where(i => i > 0).Distinct().ToArray();

        policy.RegionIds = Clean(policy.RegionIds);
        policy.ContinentIds = Clean(policy.ContinentIds);
        policy.CountryIds = Clean(policy.CountryIds);
        policy.DisabledCountryIds = Clean(policy.DisabledCountryIds);
    }
}
