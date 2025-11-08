using System.Net;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Kernel.Billing;
using Cinturon360.Shared.Models.Kernel.Platform;
using Cinturon360.Shared.Models.Static.Platform;
using Cinturon360.Shared.Services.Interfaces.Kernel;
using Cinturon360.Shared.Services.Interfaces.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HtmlAgilityPack;
using Cinturon360.Shared.Models.Static.SysVar;
using Cinturon360.Shared.Models.Static.Billing;
using Cinturon360.Shared.Models.DTOs;

namespace Cinturon360.Shared.Services.Platform;

internal sealed class AdminOrgServiceUnified : IAdminOrgServiceUnified
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AdminOrgServiceUnified> _log;
    private readonly ILoggerService _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string AUBaseUrl = "https://abr.business.gov.au";

    public AdminOrgServiceUnified(
        ApplicationDbContext db,
        ILogger<AdminOrgServiceUnified> log,
        ILoggerService logger,
        IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _log = log;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
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

        await _logger.InformationAsync(
            evt: "ORG_CREATE",
            cat: SysLogCatType.Data,
            act: SysLogActionType.Create,
            message: $"Created Organization '{org.Name}' (ID: {org.Id})",
            ent: nameof(req.Name),
            entId: org.Id,
            org: org.Id?.ToString());


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
            await _logger.InformationAsync(
                evt: "ORG_CREATE_END",
                cat: SysLogCatType.Data,
                act: SysLogActionType.End,
                message: $"CreateOrgAsync succeeded for Org '{agg.Org.Name}' (ID: {agg.Org.Id})",
                ent: nameof(req.Name),
                entId: agg.Org.Id,
                org: agg.Org.Id?.ToString());
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
            .AsNoTracking()
            .AsSplitQuery()  // avoids cartesian explosion with Includes
            .Include(o => o.Domains)
            .Include(o => o.LicenseAgreement)
            .Include(o => o.TravelPolicies)   // load the travel policies #36
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        await _logger.InformationAsync(
            evt: "ORG_READ_BY_ID",
            cat: SysLogCatType.Data,
            act: SysLogActionType.Read,
            message: $"Retrieved organization by ID: {id}",
            ent: nameof(id),
            entId: id,
            org: id?.ToString());

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

    public async Task<IReadOnlyList<IAdminOrgServiceUnified.OrganizationPickerDto>> GetAllForPickerAsync(CancellationToken ct = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name).ThenBy(o => o.Id)
            .Select(o => new IAdminOrgServiceUnified.OrganizationPickerDto
            {
                Id = o.Id,
                Name = o.Name,
                Type = o.Type,
                IsActive = o.IsActive,

                ContactPersonFirstName = o.ContactPersonFirstName,
                ContactPersonLastName  = o.ContactPersonLastName,
                ContactPersonEmail     = o.ContactPersonEmail,
                ContactPersonPhone     = o.ContactPersonPhone,

                BillingPersonFirstName = o.BillingPersonFirstName,
                BillingPersonLastName  = o.BillingPersonLastName,
                BillingPersonEmail     = o.BillingPersonEmail,
                BillingPersonPhone     = o.BillingPersonPhone,

                AdminPersonFirstName   = o.AdminPersonFirstName,
                AdminPersonLastName    = o.AdminPersonLastName,
                AdminPersonPhone       = o.AdminPersonPhone,
                AdminPersonEmail       = o.AdminPersonEmail,

                TaxId                  = o.TaxId,
                Country                = o.Country
            })
            .ToListAsync(ct);
    }

    // -------------- UPDATE --------------
    public async Task<IAdminOrgServiceUnified.OrgAggregate> UpdateAsync(IAdminOrgServiceUnified.UpdateOrgRequest req, CancellationToken ct = default)
    {
        _db.ChangeTracker.Clear();
        
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

    public async Task<bool> UpdateOrgAsync(OrganizationUnified req, CancellationToken ct = default)
    {
        // Normalize inputs (avoid empty-string FK values)
        req.ParentOrganizationId = string.IsNullOrWhiteSpace(req.ParentOrganizationId)
            ? null
            : req.ParentOrganizationId.Trim();

        req.Name = req.Name.Trim(); // (optional) keep your name tidy

        // ensure no stale tracked instances remain for this context
        _db.ChangeTracker.Clear();

        // Attach only the root entity; do NOT call Update() (avoids walking the graph)
        _db.Attach(req);
        var e = _db.Entry(req);

        // Baseline: nothing modified
        e.State = EntityState.Unchanged;

        // Mark ONLY scalar properties as modified (including FKs like ParentOrganizationId)
        foreach (var p in e.Properties)
        {
            // Skip keys, concurrency tokens, and insert-only columns
            if (p.Metadata.IsKey()) continue;
            if (p.Metadata.IsConcurrencyToken) continue;
            if (p.Metadata.Name == nameof(OrganizationUnified.CreatedAt)) continue;

            // IMPORTANT: DO NOT skip foreign keys — we want ParentOrganizationId to persist
            p.IsModified = true;
        }

        // LastUpdatedUtc column
        e.Property(x => x.LastUpdatedUtc).CurrentValue = DateTime.UtcNow;
        e.Property(x => x.LastUpdatedUtc).IsModified = true;

        // Ensure navs/collections are untouched (we never called Update(), so they're fine)
        // No need for a second pass that reverts FKs.

        // Execute single UPDATE ... WHERE Id = req.Id
        var affected = await _db.SaveChangesAsync(ct);
        return affected > 0;
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

            // NEW: set org ➜ license back-link in the same transaction
            // (Id is client-generated via Nanoid, so it's already available)
            org.LicenseAgreementId = model.Id;
            org.LicenseAgreement   = model; // optional, keeps nav in sync while tracked
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

    public async Task<bool> ValidateTaxIdAsync(string orgId, string taxId, string taxIdType, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(taxId)) throw new ArgumentException("taxId is required", nameof(taxId));
        var tnorm = taxId.Trim();

        bool taxResultStatus = false;

        switch (taxIdType)
        {
            case "AU ABN" or "AU ACN":
                {
                    var url = $"{AUBaseUrl}/ABN/View?id={taxId}";
                    var httpClient = _httpClientFactory.CreateClient();
                    var html = await httpClient.GetStringAsync(url);

                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    string GetField(string label)
                    {
                        // grab all <th> nodes once
                        var thNodes = doc.DocumentNode.SelectNodes("//th");
                        if (thNodes == null)
                            return "Not found";

                        foreach (var th in thNodes)
                        {
                            var decodedText = WebUtility.HtmlDecode(th.InnerText).Trim();
                            if (decodedText == label)
                            {
                                // pick up the next <td>
                                var td = th.SelectSingleNode("following-sibling::td");
                                if (td != null)
                                    return WebUtility.HtmlDecode(td.InnerText).Trim();
                            }
                        }

                        return "Not found";
                    }

                    var gstStatus = GetField("Goods & Services Tax (GST):");
                    taxResultStatus = gstStatus.Contains("Not currently registered for GST") || gstStatus.Contains("Not found")
                        ? false
                        : true;
                    break;
                }

            default:
                break;
        }

        if (taxResultStatus)
        {
            var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == orgId, ct)
                ?? throw new InvalidOperationException($"Organization '{orgId}' not found.");

            var now = DateTime.UtcNow;
            org.TaxLastValidated = now;
            org.LastUpdatedUtc = now;
            await _db.SaveChangesAsync(ct);

            await _logger.InformationAsync(
                evt: "ORG_TAX_ID_VALIDATE",
                cat: SysLogCatType.Tax,
                act: SysLogActionType.Validate,
                message: $"Validated Tax ID for Org '{org.Name}' (ID: {org.Id})",
                ent: nameof(org.Id),
                entId: org.Id,
                org: org.Id?.ToString());
        }

        return taxResultStatus;
    }

    public async Task<string?> GetOrgDefaultTravelPolicyIdAsync(string orgId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orgId))
        {
            await _logger.ErrorAsync(
                evt: "ORG_DEFAULT_POLICY_MISSING",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Validate,
                ex: new InvalidOperationException($"Organization {orgId} has no Default Travel Policy configured."),
                message: $"Organization {orgId} has no Default Travel Policy configured.",
                ent: nameof(orgId),
                entId: orgId,
                org: orgId?.ToString(),
                note: "default_policy_missing");
            return null;
        }

        var org = await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        return org?.DefaultTravelPolicyId ?? null;
    }

    /// <summary>
    /// Get PNR Service Fees for the given organization
    /// </summary>
    /// <param name="orgId"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<OrgFeesMarkupDto?> GetOrgPnrServiceFeesAsync(string orgId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orgId)) return null;

        var org = await _db.Organizations
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orgId, ct);

        if (org == null)
            return null;

        if (org.LicenseAgreementId == null)
            return null;

        var licenseAgreement = await _db.LicenseAgreements.AsNoTracking().FirstOrDefaultAsync(la => la.Id == org.LicenseAgreementId, ct);
        if (licenseAgreement == null)
        {
            await _logger.ErrorAsync(
                evt: "ORG_PNR_FEES_MISSING",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Validate,
                ex: new InvalidOperationException($"Organization {orgId} has no PNR Service Fees configured."),
                message: $"Organization {orgId} has no PNR Service Fees configured.",
                ent: nameof(orgId),
                entId: orgId,
                org: orgId?.ToString(),
                note: "pnr_fees_missing");

            return null;
        }

        OrgFeesMarkupDto orgFees = new OrgFeesMarkupDto
        {
            PnrCreationFee = licenseAgreement.PnrCreationFee,
            PnrChangeFee = licenseAgreement.PnrChangeFee,

            FlightMarkupPercent = licenseAgreement.FlightMarkupPercent,
            FlightPerItemFee = licenseAgreement.FlightPerItemFee,
            FlightFeeType = licenseAgreement.FlightFeeType,

            HotelMarkupPercent = licenseAgreement.HotelMarkupPercent,
            HotelPerItemFee = licenseAgreement.HotelPerItemFee,
            HotelFeeType = licenseAgreement.HotelFeeType,

            CarMarkupPercent = licenseAgreement.CarMarkupPercent,
            CarPerItemFee = licenseAgreement.CarPerItemFee,
            CarFeeType = licenseAgreement.CarFeeType,

            RailMarkupPercent = licenseAgreement.RailMarkupPercent,
            RailPerItemFee = licenseAgreement.RailPerItemFee,
            RailFeeType = licenseAgreement.RailFeeType,

            TransferMarkupPercent = licenseAgreement.TransferMarkupPercent,
            TransferPerItemFee = licenseAgreement.TransferPerItemFee,
            TransferFeeType = licenseAgreement.TransferFeeType,

            ActivityMarkupPercent = licenseAgreement.ActivityMarkupPercent,
            ActivityPerItemFee = licenseAgreement.ActivityPerItemFee,
            ActivityFeeType = licenseAgreement.ActivityFeeType,

            TravelMarkupPercent = licenseAgreement.TravelMarkupPercent,
            TravelPerItemFee = licenseAgreement.TravelPerItemFee,
            TravelFeeType = licenseAgreement.TravelFeeType
        };

        await _logger.InformationAsync(
                evt: "ORG_PNR_FEES_RETRIEVED",
                cat: SysLogCatType.Data,
                act: SysLogActionType.Read,
                message: $"Organization {orgId} PNR Service Fees retrieved.",
                ent: nameof(orgId),
                entId: orgId,
                org: orgId?.ToString(),
                note: "pnr_fees_retrieved");

        return orgFees;
    }
}


