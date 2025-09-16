using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Static.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorLoginDemo.Shared.Services.Platform;

internal sealed class AdminOrgService : IAdminOrgService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AdminOrgService> _log;

    public AdminOrgService(ApplicationDbContext db, ILogger<AdminOrgService> log)
    {
        _db = db;
        _log = log;
    }

    // --- CREATE (rich/typed) ---
    public async Task<IAdminOrgService.OrgAggregate> CreateAsync(IAdminOrgService.CreateOrgRequest req, CancellationToken ct = default)
    {
        // Validate parent
        Organization? parent = null;
        if (!string.IsNullOrWhiteSpace(req.ParentOrganizationId))
        {
            parent = await _db.Organizations
                .FirstOrDefaultAsync(o => o.Id == req.ParentOrganizationId, ct)
                ?? throw new InvalidOperationException($"Parent org '{req.ParentOrganizationId}' not found.");
        }

        // Normalize + dedup domains
        var normalizedDomains = req.Domains
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Check domain conflicts (enforced by unique index too)
        if (normalizedDomains.Count > 0)
        {
            var existing = await _db.OrganizationDomains
                .Where(od => normalizedDomains.Contains(od.Domain))
                .Select(od => od.Domain)
                .ToListAsync(ct);

            if (existing.Count > 0)
                throw new InvalidOperationException($"These domain(s) already exist: {string.Join(", ", existing)}");
        }

        var org = new Organization
        {
            Name = req.Name.Trim(),
            Type = req.Type,
            ParentOrganizationId = parent?.Id,
            IsActive = req.IsActive
        };

        foreach (var d in normalizedDomains)
        {
            org.Domains.Add(new OrganizationDomain
            {
                Domain = d,
                Organization = org
            });
        }

        _db.Organizations.Add(org);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _log.LogError(ex, "DB error during org create");
            if (IsUniqueDomainViolation(ex))
                throw new InvalidOperationException("One or more domains already exist on another organization.");
            throw;
        }

        // Reload with domains
        var loaded = await _db.Organizations
            .Include(o => o.Domains)
            .FirstAsync(o => o.Id == org.Id, ct);

        return new IAdminOrgService.OrgAggregate(loaded, loaded.Domains.ToList());
    }

    // --- CREATE (UI-friendly result) ---
    public async Task<IAdminOrgService.CreateOrgResult> CreateOrgAsync(IAdminOrgService.CreateOrgRequest req, CancellationToken ct = default)
    {
        try
        {
            var agg = await CreateAsync(req, ct);
            return new(true, null, agg.Org.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CreateOrgAsync failed");
            return new(false, ex.GetBaseException().Message, null);
        }
    }

    // --- READ ---
    public async Task<IAdminOrgService.OrgAggregate?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var org = await _db.Organizations
            .Include(o => o.Domains)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        return org is null ? null : new IAdminOrgService.OrgAggregate(org, org.Domains.ToList());
    }

    public async Task<IReadOnlyList<IAdminOrgService.OrgAggregate>> SearchAsync(
        string? nameContains,
        OrganizationType? type,
        bool? isActive,
        string? parentOrgId,
        string? domainContains,
        CancellationToken ct = default)
    {
        IQueryable<Organization> q = _db.Organizations
            .Include(o => o.Domains)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameContains))
            q = q.Where(o => EF.Functions.ILike(o.Name, $"%{nameContains.Trim()}%"));

        if (type.HasValue)
            q = q.Where(o => o.Type == type);

        if (isActive.HasValue)
            q = q.Where(o => o.IsActive == isActive);

        if (!string.IsNullOrWhiteSpace(parentOrgId))
            q = q.Where(o => o.ParentOrganizationId == parentOrgId.Trim());

        if (!string.IsNullOrWhiteSpace(domainContains))
            q = q.Where(o => o.Domains.Any(d => EF.Functions.ILike(d.Domain, $"%{domainContains.Trim()}%")));

        var list = await q.OrderBy(o => o.Name).ThenBy(o => o.Id).ToListAsync(ct);

        return list.Select(o => new IAdminOrgService.OrgAggregate(o, o.Domains.ToList())).ToList();
    }

    // ---------------------------
    // UPDATE
    // ---------------------------
    public async Task<IAdminOrgService.OrgAggregate> UpdateAsync(
        IAdminOrgService.UpdateOrgRequest req,
        CancellationToken ct = default)
    {
        var org = await _db.Organizations
            .Include(o => o.Domains)
            .FirstOrDefaultAsync(o => o.Id == req.OrgId, ct)
            ?? throw new InvalidOperationException($"Organization '{req.OrgId}' not found.");

        // Field updates (only when provided)
        if (req.Name is not null)
        {
            var n = req.Name.Trim();
            if (n.Length == 0) throw new ArgumentException("Name cannot be empty.", nameof(req.Name));
            org.Name = n;
        }

        if (req.Type.HasValue)
            org.Type = req.Type.Value;

        if (req.ParentOrganizationId is not null)
        {
            var pid = req.ParentOrganizationId.Trim();
            if (pid.Length == 0)
            {
                org.ParentOrganizationId = null; // clearing parent
            }
            else
            {
                // validate parent exists and avoid self-parenting
                if (pid == org.Id)
                    throw new InvalidOperationException("An organization cannot be its own parent.");

                var parentExists = await _db.Organizations.AnyAsync(o => o.Id == pid, ct);
                if (!parentExists)
                    throw new InvalidOperationException($"Parent org '{pid}' not found.");

                org.ParentOrganizationId = pid;
            }
        }

        if (req.IsActive.HasValue)
            org.IsActive = req.IsActive.Value;

        // Domains replace (full replace when provided)
        if (req.DomainsReplace is not null)
        {
            var incoming = req.DomainsReplace
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Check for conflicts with other orgs
            if (incoming.Count > 0)
            {
                var conflicts = await _db.OrganizationDomains
                    .Where(od => incoming.Contains(od.Domain) && od.OrganizationId != org.Id)
                    .Select(od => od.Domain)
                    .ToListAsync(ct);

                if (conflicts.Count > 0)
                    throw new InvalidOperationException($"These domain(s) are already in use: {string.Join(", ", conflicts)}");
            }

            // Compute diffs
            var existing = org.Domains.Select(d => d.Domain).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var toAdd = incoming.Where(d => !existing.Contains(d)).ToList();
            var toRemove = org.Domains.Where(d => !incoming.Contains(d.Domain, StringComparer.OrdinalIgnoreCase)).ToList();

            // Apply removals
            if (toRemove.Count > 0)
            {
                _db.OrganizationDomains.RemoveRange(toRemove);
            }

            // Apply additions
            foreach (var d in toAdd)
            {
                org.Domains.Add(new OrganizationDomain { Domain = d, OrganizationId = org.Id });
            }
        }

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _log.LogError(ex, "DB error during org update");
            if (IsUniqueDomainViolation(ex))
                throw new InvalidOperationException("One or more domains already exist on another organization.");
            throw;
        }

        // Return fresh aggregate
        var refreshed = await _db.Organizations
            .Include(o => o.Domains)
            .FirstAsync(o => o.Id == org.Id, ct);

        return new IAdminOrgService.OrgAggregate(refreshed, refreshed.Domains.ToList());
    }

    public async Task<IAdminOrgService.OrgAggregate> RemoveDomainAsync(
        string orgId,
        string domain,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orgId))
            throw new ArgumentException("orgId is required", nameof(orgId));
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("domain is required", nameof(domain));

        var dnorm = domain.Trim().ToLowerInvariant();

        var org = await _db.Organizations
            .Include(o => o.Domains)
            .FirstOrDefaultAsync(o => o.Id == orgId, ct)
            ?? throw new InvalidOperationException($"Organization '{orgId}' not found.");

        var toRemove = org.Domains
            .FirstOrDefault(d => string.Equals(d.Domain, dnorm, StringComparison.OrdinalIgnoreCase));

        if (toRemove is null)
        {
            // Nothing to do; return current state
            return new IAdminOrgService.OrgAggregate(org, org.Domains.ToList());
        }

        // Remove from nav + DbSet (be explicit)
        org.Domains.Remove(toRemove);
        _db.OrganizationDomains.Remove(toRemove);

        try
        {
            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            _log.LogError(ex, "RemoveDomainAsync failed");
            throw new InvalidOperationException("Failed to remove domain.", ex);
        }

        // Return fresh state
        var refreshed = await _db.Organizations
            .Include(o => o.Domains)
            .FirstAsync(o => o.Id == orgId, ct);

        return new IAdminOrgService.OrgAggregate(refreshed, refreshed.Domains.ToList());
    }

    // --- UTIL ---
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.Organizations.AnyAsync(o => o.Id == id, ct);

    private static bool IsUniqueDomainViolation(DbUpdateException ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        return msg.Contains("organization_domain", StringComparison.OrdinalIgnoreCase)
            && msg.Contains("unique", StringComparison.OrdinalIgnoreCase);
    }
}
