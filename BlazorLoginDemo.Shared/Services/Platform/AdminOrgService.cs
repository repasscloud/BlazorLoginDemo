using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Models.Static.Platform;
using BlazorLoginDemo.Shared.Services.Interfaces.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlazorLoginDemo.Shared.Services.Platform;

internal sealed class AdminOrgServiceUnified : IAdminOrgServiceUnified
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AdminOrgServiceUnified> _log;

    public AdminOrgServiceUnified(ApplicationDbContext db, ILogger<AdminOrgServiceUnified> log)
    {
        _db = db;
        _log = log;
    }

    // -------------- CREATE (rich) --------------
    public async Task<IAdminOrgServiceUnified.OrgAggregate> CreateAsync(IAdminOrgServiceUnified.CreateOrgRequest req, CancellationToken ct = default)
    {
        // Validate parent (optional)
        OrganizationUnified? parent = null;
        if (!string.IsNullOrWhiteSpace(req.ParentOrganizationId))
        {
            parent = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == req.ParentOrganizationId, ct)
                ?? throw new InvalidOperationException($"Parent org '{req.ParentOrganizationId}' not found.");
        }

        // Normalize + dedup domains
        var normalizedDomains = req.Domains
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => d.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Check existing domain conflicts
        if (normalizedDomains.Count > 0)
        {
            var existing = await _db.OrganizationDomains
                .Where(od => normalizedDomains.Contains(od.Domain))
                .Select(od => od.Domain)
                .ToListAsync(ct);
            if (existing.Count > 0)
                throw new InvalidOperationException($"These domain(s) already exist: {string.Join(", ", existing)}");
        }

        var org = new OrganizationUnified
        {
            Name = req.Name.Trim(),
            Type = req.Type,
            ParentOrganizationId = parent?.Id,
            IsActive = req.IsActive
        };

        foreach (var d in normalizedDomains)
        {
            org.Domains.Add(new OrganizationDomainUnified
            {
                Domain = d,
                Organization = org
            });
        }

        _db.Organizations.Add(org);

        try { await _db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex)
        {
            _log.LogError(ex, "DB error during org create");
            throw;
        }

        var loaded = await _db.Organizations
            .Include(o => o.Domains)
            .Include(o => o.LicenseAgreement)
            .FirstAsync(o => o.Id == org.Id, ct);

        return new IAdminOrgServiceUnified.OrgAggregate(loaded, loaded.Domains.ToList(), loaded.LicenseAgreement);
    }

    public async Task<IAdminOrgServiceUnified.CreateOrgResult> CreateOrgAsync(IAdminOrgServiceUnified.CreateOrgRequest req, CancellationToken ct = default)
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

    // -------------- READ / SEARCH --------------
    public async Task<IAdminOrgServiceUnified.OrgAggregate?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var org = await _db.Organizations
            .Include(o => o.Domains)
            .Include(o => o.LicenseAgreement)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        return org is null ? null : new IAdminOrgServiceUnified.OrgAggregate(org, org.Domains.ToList(), org.LicenseAgreement);
    }

    public async Task<IReadOnlyList<IAdminOrgServiceUnified.OrgAggregate>> SearchAsync(
        string? nameContains,
        OrganizationType? type,
        bool? isActive,
        string? parentOrgId,
        string? domainContains,
        CancellationToken ct = default)
    {
        IQueryable<OrganizationUnified> q = _db.Organizations
            .Include(o => o.Domains)
            .Include(o => o.LicenseAgreement)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(nameContains))
            q = q.Where(o => EF.Functions.ILike(o.Name, $"%{nameContains.Trim()}%"));
        if (type.HasValue) q = q.Where(o => o.Type == type.Value);
        if (isActive.HasValue) q = q.Where(o => o.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(parentOrgId)) q = q.Where(o => o.ParentOrganizationId == parentOrgId.Trim());
        if (!string.IsNullOrWhiteSpace(domainContains)) q = q.Where(o => o.Domains.Any(d => EF.Functions.ILike(d.Domain, $"%{domainContains.Trim()}%")));

        var list = await q.OrderBy(o => o.Name).ThenBy(o => o.Id).ToListAsync(ct);
        return list.Select(o => new IAdminOrgServiceUnified.OrgAggregate(o, o.Domains.ToList(), o.LicenseAgreement)).ToList();
    }


    // -------------- UPDATE --------------
    public async Task<IAdminOrgServiceUnified.OrgAggregate> UpdateAsync(IAdminOrgServiceUnified.UpdateOrgRequest req, CancellationToken ct = default)
    {
        var org = await _db.Organizations
            .Include(o => o.Domains)
            .Include(o => o.LicenseAgreement)
            .FirstOrDefaultAsync(o => o.Id == req.OrgId, ct)
            ?? throw new InvalidOperationException($"Organization '{req.OrgId}' not found.");

        if (req.Name is not null)
        {
            var n = req.Name.Trim();
            if (n.Length == 0) throw new ArgumentException("Name cannot be empty.", nameof(req.Name));
            org.Name = n;
        }
        if (req.Type.HasValue) org.Type = req.Type.Value;
        if (req.ParentOrganizationId is not null)
        {
            var pid = req.ParentOrganizationId.Trim();
            if (pid.Length == 0) org.ParentOrganizationId = null; // clear
            else
            {
                if (pid == org.Id) throw new InvalidOperationException("An organization cannot be its own parent.");
                var exists = await _db.Organizations.AnyAsync(o => o.Id == pid, ct);
                if (!exists) throw new InvalidOperationException($"Parent org '{pid}' not found.");
                org.ParentOrganizationId = pid;
            }
        }
        if (req.IsActive.HasValue) org.IsActive = req.IsActive.Value;

        if (req.DomainsReplace is not null)
        {
            var incoming = req.DomainsReplace
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (incoming.Count > 0)
            {
                var conflicts = await _db.OrganizationDomains
                    .Where(od => incoming.Contains(od.Domain) && od.OrganizationUnifiedId != org.Id)
                    .Select(od => od.Domain)
                    .ToListAsync(ct);
                if (conflicts.Count > 0)
                    throw new InvalidOperationException($"These domain(s) are already in use: {string.Join(", ", conflicts)}");
            }

            var existing = org.Domains.Select(d => d.Domain).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var toAdd = incoming.Where(d => !existing.Contains(d)).ToList();
            var toRemove = org.Domains.Where(d => !incoming.Contains(d.Domain, StringComparer.OrdinalIgnoreCase)).ToList();

            if (toRemove.Count > 0) _db.OrganizationDomains.RemoveRange(toRemove);
            foreach (var d in toAdd)
                org.Domains.Add(new OrganizationDomainUnified { Domain = d, OrganizationUnifiedId = org.Id });
        }

        await _db.SaveChangesAsync(ct);

        var refreshed = await _db.Organizations
            .Include(o => o.Domains)
            .Include(o => o.LicenseAgreement)
            .FirstAsync(o => o.Id == org.Id, ct);
        return new IAdminOrgServiceUnified.OrgAggregate(refreshed, refreshed.Domains.ToList(), refreshed.LicenseAgreement);
    }

    public async Task<IAdminOrgServiceUnified.OrgAggregate> RemoveDomainAsync(string orgId, string domain, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orgId)) throw new ArgumentException("orgId is required", nameof(orgId));
        if (string.IsNullOrWhiteSpace(domain)) throw new ArgumentException("domain is required", nameof(domain));

        var dnorm = domain.Trim().ToLowerInvariant();
        var org = await _db.Organizations.Include(o => o.Domains).Include(o => o.LicenseAgreement)
            .FirstOrDefaultAsync(o => o.Id == orgId, ct)
            ?? throw new InvalidOperationException($"Organization '{orgId}' not found.");

        var toRemove = org.Domains.FirstOrDefault(d => string.Equals(d.Domain, dnorm, StringComparison.OrdinalIgnoreCase));
        if (toRemove is null)
            return new IAdminOrgServiceUnified.OrgAggregate(org, org.Domains.ToList(), org.LicenseAgreement);

        org.Domains.Remove(toRemove);
        _db.OrganizationDomains.Remove(toRemove);
        await _db.SaveChangesAsync(ct);

        var refreshed = await _db.Organizations.Include(o => o.Domains).Include(o => o.LicenseAgreement)
            .FirstAsync(o => o.Id == orgId, ct);
        return new IAdminOrgServiceUnified.OrgAggregate(refreshed, refreshed.Domains.ToList(), refreshed.LicenseAgreement);
    }

    public async Task<IAdminOrgServiceUnified.OrgAggregate> UpsertLicenseAgreementAsync(string orgId, LicenseAgreementUnified model, CancellationToken ct = default)
    {
        var org = await _db.Organizations.Include(o => o.LicenseAgreement).FirstOrDefaultAsync(o => o.Id == orgId, ct)
            ?? throw new InvalidOperationException($"Organization '{orgId}' not found.");

        if (org.LicenseAgreement is null)
        {
            // create new and attach 1:1
            model.OrganizationUnifiedId = org.Id;
            _db.LicenseAgreements.Add(model);
        }
        else
        {
            // update fields (shallow copy; adjust as needed)
            var la = org.LicenseAgreement;
            la.StartDate = model.StartDate;
            la.ExpiryDate = model.ExpiryDate;
            la.RenewalDate = model.RenewalDate;
            la.RemittanceEmail = model.RemittanceEmail;
            la.PaymentTerms = model.PaymentTerms;
            la.PaymentMethod = model.PaymentMethod;
            la.BillingType = model.BillingType;
            la.BillingFrequency = model.BillingFrequency;
            la.AutoRenew = model.AutoRenew;
            la.AccessFee = model.AccessFee;
            la.AccessFeeScope = model.AccessFeeScope;
            la.AccountThreshold = model.AccountThreshold;
            la.ThresholdScope = model.ThresholdScope;
            la.TaxRate = model.TaxRate;
            la.MinimumMonthlySpend = model.MinimumMonthlySpend;
            la.PrepaidBalance = model.PrepaidBalance;
            la.GracePeriodDays = model.GracePeriodDays;
            la.DiscountA = model.DiscountA;
            la.DiscountB = model.DiscountB;
            la.TrialEndsOnUtc = model.TrialEndsOnUtc;
            la.PnrCreationFee = model.PnrCreationFee;
            la.PnrChangeFee = model.PnrChangeFee;
            la.FlightMarkupPercent = model.FlightMarkupPercent; la.FlightPerItemFee = model.FlightPerItemFee; la.FlightFeeType = model.FlightFeeType;
            la.HotelMarkupPercent = model.HotelMarkupPercent; la.HotelPerItemFee = model.HotelPerItemFee; la.HotelFeeType = model.HotelFeeType;
            la.CarMarkupPercent = model.CarMarkupPercent; la.CarPerItemFee = model.CarPerItemFee; la.CarFeeType = model.CarFeeType;
            la.RailMarkupPercent = model.RailMarkupPercent; la.RailPerItemFee = model.RailPerItemFee; la.RailFeeType = model.RailFeeType;
            la.TransferMarkupPercent = model.TransferMarkupPercent; la.TransferPerItemFee = model.TransferPerItemFee; la.TransferFeeType = model.TransferFeeType;
            la.ActivityMarkupPercent = model.ActivityMarkupPercent; la.ActivityPerItemFee = model.ActivityPerItemFee; la.ActivityFeeType = model.ActivityFeeType;
            la.TravelMarkupPercent = model.TravelMarkupPercent; la.TravelPerItemFee = model.TravelPerItemFee; la.TravelFeeType = model.TravelFeeType;
            la.LateFees = model.LateFees;
            la.ClientCountLimit = model.ClientCountLimit;
            la.UserAccountLimit = model.UserAccountLimit;
            la.PaymentStatus = model.PaymentStatus;
            la.LastUpdatedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(ct);
        var refreshed = await _db.Organizations.Include(o => o.Domains).Include(o => o.LicenseAgreement)
            .FirstAsync(o => o.Id == orgId, ct);
        return new IAdminOrgServiceUnified.OrgAggregate(refreshed, refreshed.Domains.ToList(), refreshed.LicenseAgreement);
    }

    public async Task<bool> DeleteLicenseAgreementAsync(string orgId, CancellationToken ct = default)
    {
        var org = await _db.Organizations.Include(o => o.LicenseAgreement).FirstOrDefaultAsync(o => o.Id == orgId, ct);
        if (org?.LicenseAgreement is null) return false;
        _db.LicenseAgreements.Remove(org.LicenseAgreement);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.Organizations.AnyAsync(o => o.Id == id, ct);
}
