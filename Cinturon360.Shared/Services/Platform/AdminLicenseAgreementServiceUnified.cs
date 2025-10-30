using System.Linq.Expressions;
using Cinturon360.Shared.Data;
using Cinturon360.Shared.Models.Kernel.Billing;
using Cinturon360.Shared.Models.Kernel.Platform;
using Cinturon360.Shared.Services.Interfaces.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cinturon360.Shared.Services.Platform;

internal sealed class AdminLicenseAgreementServiceUnified : IAdminLicenseAgreementServiceUnified
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<AdminLicenseAgreementServiceUnified> _log;

    public AdminLicenseAgreementServiceUnified(ApplicationDbContext db, ILogger<AdminLicenseAgreementServiceUnified> log)
    {
        _db  = db;
        _log = log;
    }

    // -------------------- Helpers --------------------
    private static void ValidateDates(LicenseAgreementUnified m)
    {
        if (m.ExpiryDate < m.StartDate)
            throw new ArgumentException("ExpiryDate cannot be before StartDate.");
        if (m.RenewalDate is { } r && r < m.ExpiryDate)
            throw new ArgumentException("RenewalDate cannot be before ExpiryDate.");
    }

    private async Task<(OrganizationUnified org, OrganizationUnified issuer)> EnsureOrgLinksAsync(
        string organizationId, string issuerOrgId, CancellationToken ct)
    {
        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == organizationId, ct)
            ?? throw new InvalidOperationException($"Organization '{organizationId}' not found.");
        var issuer = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == issuerOrgId, ct)
            ?? throw new InvalidOperationException($"Issuer organization '{issuerOrgId}' not found.");
        return (org, issuer);
    }

    private static IAdminLicenseAgreementServiceUnified.LicenseAggregate ToAgg(
        LicenseAgreementUnified la, OrganizationUnified org, OrganizationUnified issuer)
        => new(la, org, issuer);

    // -------------------- CREATE / UPSERT --------------------
    public async Task<IAdminLicenseAgreementServiceUnified.LicenseAggregate> CreateAsync(
        IAdminLicenseAgreementServiceUnified.CreateRequest req, CancellationToken ct = default)
    {
        var (org, issuer) = await EnsureOrgLinksAsync(req.OrganizationUnifiedId, req.CreatedByOrganizationUnifiedId, ct);

        var model = req.Model ?? new LicenseAgreementUnified
        {
            OrganizationUnifiedId = org.Id,
            CreatedByOrganizationUnifiedId = issuer.Id,
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.Date).AddYears(1),
            AutoRenew = false,
            LastUpdatedAtUtc = DateTime.UtcNow,
            CreatedAtUtc = DateTime.UtcNow
        };

        // enforce org links (caller may have supplied Model with wrong links)
        model.OrganizationUnifiedId = org.Id;
        model.CreatedByOrganizationUnifiedId = issuer.Id;

        ValidateDates(model);

        _db.LicenseAgreements.Add(model);
        try { await _db.SaveChangesAsync(ct); }
        catch (DbUpdateException ex)
        {
            _log.LogError(ex, "DB error during license create");
            throw;
        }

        // NEW: back-link the Organization to this License (ava.organizations.LicenseAgreementId)
        await _db.Organizations
            .Where(o => o.Id == org.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, model.Id), ct);

        // Load & return
        var loaded = await _db.LicenseAgreements.AsNoTracking().FirstAsync(x => x.Id == model.Id, ct);
        return ToAgg(loaded, org, issuer);
    }

    public async Task<IAdminLicenseAgreementServiceUnified.CreateResult> CreateLicenseAsync(
        IAdminLicenseAgreementServiceUnified.CreateRequest req, CancellationToken ct = default)
    {
        try
        {
            var agg = await CreateAsync(req, ct);
            return new(true, null, agg.License.Id);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CreateLicenseAsync failed");
            return new(false, ex.GetBaseException().Message, null);
        }
    }

    public async Task<IAdminLicenseAgreementServiceUnified.LicenseAggregate> UpsertForOrganizationAsync(
        string organizationUnifiedId,
        string createdByOrganizationUnifiedId,
        LicenseAgreementUnified model,
        CancellationToken ct = default)
    {
        var (org, issuer) = await EnsureOrgLinksAsync(organizationUnifiedId, createdByOrganizationUnifiedId, ct);

        // Does org already have a license?
        var existing = await _db.LicenseAgreements
            .Include(x => x.Organization)
            .Include(x => x.CreatedByOrganization)
            .FirstOrDefaultAsync(x => x.OrganizationUnifiedId == org.Id, ct);

        if (existing is null)
        {
            model.OrganizationUnifiedId = org.Id;
            model.CreatedByOrganizationUnifiedId = issuer.Id;
            model.CreatedAtUtc = DateTime.UtcNow;
            model.LastUpdatedAtUtc = DateTime.UtcNow;
            ValidateDates(model);
            _db.LicenseAgreements.Add(model);
            await _db.SaveChangesAsync(ct);

            // NEW: back-link org ➜ license
            await _db.Organizations
                .Where(o => o.Id == org.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, model.Id), ct);

            var created = await _db.LicenseAgreements.AsNoTracking().FirstAsync(x => x.Id == model.Id, ct);
            return ToAgg(created, org, issuer);
        }
        else
        {
            // shallow copy of scalars (do not reassign navs directly)
            ValidateDates(model);

            existing.StartDate = model.StartDate;
            existing.ExpiryDate = model.ExpiryDate;
            existing.RenewalDate = model.RenewalDate;
            existing.RemittanceEmail = model.RemittanceEmail;
            existing.PaymentTerms = model.PaymentTerms;
            existing.PaymentMethod = model.PaymentMethod;
            existing.BillingType = model.BillingType;
            existing.BillingFrequency = model.BillingFrequency;
            existing.AutoRenew = model.AutoRenew;
            existing.AccessFee = model.AccessFee;
            existing.AccessFeeScope = model.AccessFeeScope;
            existing.AccountThreshold = model.AccountThreshold;
            existing.ThresholdScope = model.ThresholdScope;
            existing.TaxRate = model.TaxRate;
            existing.MinimumMonthlySpend = model.MinimumMonthlySpend;
            existing.PrepaidBalance = model.PrepaidBalance;
            existing.GracePeriodDays = model.GracePeriodDays;
            existing.DiscountA = model.DiscountA;
            existing.DiscountB = model.DiscountB;
            existing.TrialEndsOnUtc = model.TrialEndsOnUtc;
            existing.PnrCreationFee = model.PnrCreationFee;
            existing.PnrChangeFee = model.PnrChangeFee;

            existing.FlightMarkupPercent = model.FlightMarkupPercent; existing.FlightPerItemFee = model.FlightPerItemFee; existing.FlightFeeType = model.FlightFeeType;
            existing.HotelMarkupPercent  = model.HotelMarkupPercent;  existing.HotelPerItemFee  = model.HotelPerItemFee;  existing.HotelFeeType  = model.HotelFeeType;
            existing.CarMarkupPercent    = model.CarMarkupPercent;    existing.CarPerItemFee    = model.CarPerItemFee;    existing.CarFeeType    = model.CarFeeType;
            existing.RailMarkupPercent   = model.RailMarkupPercent;   existing.RailPerItemFee   = model.RailPerItemFee;   existing.RailFeeType   = model.RailFeeType;
            existing.TransferMarkupPercent = model.TransferMarkupPercent; existing.TransferPerItemFee = model.TransferPerItemFee; existing.TransferFeeType = model.TransferFeeType;
            existing.ActivityMarkupPercent = model.ActivityMarkupPercent; existing.ActivityPerItemFee = model.ActivityPerItemFee; existing.ActivityFeeType = model.ActivityFeeType;
            existing.TravelMarkupPercent   = model.TravelMarkupPercent;   existing.TravelPerItemFee   = model.TravelPerItemFee;   existing.TravelFeeType   = model.TravelFeeType;

            existing.LateFees = model.LateFees;
            existing.ClientCountLimit = model.ClientCountLimit;
            existing.UserAccountLimit = model.UserAccountLimit;
            existing.PaymentStatus = model.PaymentStatus;

            existing.OrganizationUnifiedId = org.Id;
            existing.CreatedByOrganizationUnifiedId = issuer.Id;
            existing.LastUpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);
            // NEW: ensure org ➜ license back-link remains correct
            await _db.Organizations
                .Where(o => o.Id == org.Id)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, existing.Id), ct);

            var refreshed = await _db.LicenseAgreements.AsNoTracking().FirstAsync(x => x.Id == existing.Id, ct);
            return ToAgg(refreshed, org, issuer);
        }
    }

    // ------------------------- READ -------------------------
    public async Task<IAdminLicenseAgreementServiceUnified.LicenseAggregate?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var la = await _db.LicenseAgreements
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Organization)
            .Include(x => x.CreatedByOrganization)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        return la is null ? null : ToAgg(la, la.Organization, la.CreatedByOrganization);
    }

    public async Task<IAdminLicenseAgreementServiceUnified.LicenseAggregate?> GetByOrganizationIdAsync(
        string organizationUnifiedId, CancellationToken ct = default)
    {
        var la = await _db.LicenseAgreements
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Organization)
            .Include(x => x.CreatedByOrganization)
            .FirstOrDefaultAsync(x => x.OrganizationUnifiedId == organizationUnifiedId, ct);

        return la is null ? null : ToAgg(la, la.Organization, la.CreatedByOrganization);
    }

    public async Task<IReadOnlyList<IAdminLicenseAgreementServiceUnified.LicenseAggregate>> SearchAsync(
        IAdminLicenseAgreementServiceUnified.SearchParams f, CancellationToken ct = default)
    {
        IQueryable<LicenseAgreementUnified> q = _db.LicenseAgreements
            .AsNoTracking()
            .AsSplitQuery()
            .Include(x => x.Organization)
            .Include(x => x.CreatedByOrganization);

        if (!string.IsNullOrWhiteSpace(f.OrganizationId))
            q = q.Where(x => x.OrganizationUnifiedId == f.OrganizationId);
        if (!string.IsNullOrWhiteSpace(f.IssuerOrganizationId))
            q = q.Where(x => x.CreatedByOrganizationUnifiedId == f.IssuerOrganizationId);
        if (f.StartsOnOrAfter.HasValue)
            q = q.Where(x => x.StartDate >= f.StartsOnOrAfter.Value);
        if (f.ExpiresOnOrBefore.HasValue)
            q = q.Where(x => x.ExpiryDate <= f.ExpiresOnOrBefore.Value);
        if (f.AutoRenew.HasValue)
            q = q.Where(x => x.AutoRenew == f.AutoRenew.Value);
        if (f.MinPrepaidBalanceGte.HasValue)
            q = q.Where(x => x.PrepaidBalance >= f.MinPrepaidBalanceGte.Value);
        if (f.MinMonthlySpendGte.HasValue)
            q = q.Where(x => (x.MinimumMonthlySpend ?? 0m) >= f.MinMonthlySpendGte.Value);
        if (f.PaymentStatus.HasValue)
            q = q.Where(x => x.PaymentStatus == f.PaymentStatus.Value);

        var list = await q.OrderBy(x => x.Organization.Name).ThenBy(x => x.Id).ToListAsync(ct);
        return list.Select(x => ToAgg(x, x.Organization, x.CreatedByOrganization)).ToList();
    }

    public async Task<IReadOnlyList<IAdminLicenseAgreementServiceUnified.PickerDto>> GetAllForPickerAsync(CancellationToken ct = default)
    {
        return await _db.LicenseAgreements
            .AsNoTracking()
            .OrderBy(x => x.Organization.Name).ThenBy(x => x.Id)
            .Select(x => new IAdminLicenseAgreementServiceUnified.PickerDto
            {
                Id = x.Id,
                OrganizationId = x.OrganizationUnifiedId,
                OrganizationName = x.Organization.Name,
                IssuerOrganizationId = x.CreatedByOrganizationUnifiedId,
                IssuerOrganizationName = x.CreatedByOrganization.Name,
                StartDate = x.StartDate,
                ExpiryDate = x.ExpiryDate,
                AutoRenew = x.AutoRenew,
                PaymentStatus = x.PaymentStatus,
                PrepaidBalance = x.PrepaidBalance
            })
            .ToListAsync(ct);
    }

    // ------------------------ UPDATE ------------------------
    public async Task<bool> UpdateAsync(LicenseAgreementUnified replacement, CancellationToken ct = default)
    {
        if (replacement is null) throw new ArgumentNullException(nameof(replacement));
        ValidateDates(replacement);

        // Validate (and normalize) org links if they are provided
        await EnsureOrgLinksAsync(replacement.OrganizationUnifiedId, replacement.CreatedByOrganizationUnifiedId, ct);

        // Avoid graph fix-up
        _db.ChangeTracker.Clear();

        // Attach root only
        _db.Attach(replacement);
        var e = _db.Entry(replacement);

        // Do not walk navs; baseline Unchanged
        e.State = EntityState.Unchanged;

        // Mark ONLY scalar properties modified (including FKs for the two orgs)
        foreach (var p in e.Properties)
        {
            if (p.Metadata.IsKey()) continue;
            if (p.Metadata.IsConcurrencyToken) continue;
            if (p.Metadata.Name == nameof(LicenseAgreementUnified.CreatedAtUtc)) continue;

            // We let EF update complex/owned scalars like LateFees/Discounts via full column write
            p.IsModified = true;
        }

        e.Property(x => x.LastUpdatedAtUtc).CurrentValue = DateTime.UtcNow;
        e.Property(x => x.LastUpdatedAtUtc).IsModified = true;

        var affected = await _db.SaveChangesAsync(ct);

        // NEW: keep org ➜ license back-link consistent
        await _db.Organizations
            .Where(o => o.LicenseAgreementId == replacement.Id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, (string?)null), ct);

        await _db.Organizations
            .Where(o => o.Id == replacement.OrganizationUnifiedId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, replacement.Id), ct);

        return affected > 0;
    }

    public async Task<bool> UpdateFieldAsync<T>(
        string id,
        Expression<Func<LicenseAgreementUnified, T>> property,
        T value,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("id is required.", nameof(id));
        if (property is null)
            throw new ArgumentNullException(nameof(property));

        _db.ChangeTracker.Clear();

        // 1) Get the current required FKs (no-tracking)
        var current = await _db.LicenseAgreements.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.OrganizationUnifiedId, x.CreatedByOrganizationUnifiedId })
            .FirstOrDefaultAsync(ct);

        if (current is null) return false; // not found

        // 2) Work out initializer values for required members
        var member = property.Body as MemberExpression
                    ?? (property.Body as UnaryExpression)?.Operand as MemberExpression
                    ?? throw new ArgumentException("Use a simple scalar accessor like x => x.AutoRenew.", nameof(property));
        var propName = member.Member.Name;

        string orgIdInit   = current.OrganizationUnifiedId;
        string issuerIdInit = current.CreatedByOrganizationUnifiedId;

        // If caller is updating either FK, initialize with the *new* value and validate
        if (propName == nameof(LicenseAgreementUnified.OrganizationUnifiedId))
        {
            if (value is not string newOrgId || string.IsNullOrWhiteSpace(newOrgId))
                throw new ArgumentException("OrganizationUnifiedId must be a non-empty string.", nameof(value));
            orgIdInit = newOrgId;
            // validate pair
            _ = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == orgIdInit, ct)
                ?? throw new InvalidOperationException($"Organization '{orgIdInit}' not found.");
        }
        else if (propName == nameof(LicenseAgreementUnified.CreatedByOrganizationUnifiedId))
        {
            if (value is not string newIssuerId || string.IsNullOrWhiteSpace(newIssuerId))
                throw new ArgumentException("CreatedByOrganizationUnifiedId must be a non-empty string.", nameof(value));
            issuerIdInit = newIssuerId;
            _ = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == issuerIdInit, ct)
                ?? throw new InvalidOperationException($"Issuer organization '{issuerIdInit}' not found.");
        }

        // 3) Attach key-only stub with required members set (satisfies C# `required`)
        var stub = new LicenseAgreementUnified
        {
            Id = id,
            OrganizationUnifiedId = orgIdInit,
            CreatedByOrganizationUnifiedId = issuerIdInit
        };
        var entry = _db.Attach(stub); // Unchanged

        // 4) Guard against invalid targets
        var target = entry.Property(propName);
        if (target.Metadata.IsKey())
            throw new ArgumentException("Cannot update key properties.", nameof(property));
        if (target.Metadata.IsShadowProperty())
            throw new ArgumentException("Shadow properties are not supported.", nameof(property));
        if (target.Metadata.IsConcurrencyToken)
            throw new ArgumentException("Use a dedicated method for concurrency tokens.", nameof(property));

        // 5) Update only the requested column
        target.CurrentValue = value!;
        target.IsModified   = true;

        // 6) Touch LastUpdatedAtUtc if present
        var lastUpdatedProp = entry.Metadata.FindProperty(nameof(LicenseAgreementUnified.LastUpdatedAtUtc));
        if (lastUpdatedProp is not null)
        {
            entry.Property(nameof(LicenseAgreementUnified.LastUpdatedAtUtc)).CurrentValue = DateTime.UtcNow;
            entry.Property(nameof(LicenseAgreementUnified.LastUpdatedAtUtc)).IsModified   = true;
        }

        var affected = await _db.SaveChangesAsync(ct);

        // NEW: if we changed the owning org, re-point the org back-link
        if (propName == nameof(LicenseAgreementUnified.OrganizationUnifiedId))
        {
            // clear previous (only if it pointed to this license)
            await _db.Organizations
                .Where(o => o.Id == current.OrganizationUnifiedId && o.LicenseAgreementId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, (string?)null), ct);

            // set new
            await _db.Organizations
                .Where(o => o.Id == orgIdInit) // orgIdInit holds the *new* org id we validated above
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, id), ct);
        }

        return affected > 0;
    }

    public async Task<bool> ReassignOrganizationsAsync(
        string id,
        string newOrganizationUnifiedId,
        string newCreatedByOrganizationUnifiedId,
        CancellationToken ct = default)
    {
        await EnsureOrgLinksAsync(newOrganizationUnifiedId, newCreatedByOrganizationUnifiedId, ct);

        // Update the license owner/issuer
        var affected = await _db.LicenseAgreements
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.OrganizationUnifiedId, newOrganizationUnifiedId)
                .SetProperty(x => x.CreatedByOrganizationUnifiedId, newCreatedByOrganizationUnifiedId)
                .SetProperty(x => x.LastUpdatedAtUtc, DateTime.UtcNow), ct);

        // NEW: move the org back-link
        // 1) clear any previous org that points to this license
        await _db.Organizations
            .Where(o => o.LicenseAgreementId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, (string?)null), ct);

        // 2) set the new org to point to this license
        await _db.Organizations
            .Where(o => o.Id == newOrganizationUnifiedId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, id), ct);

        return affected > 0;
    }

    public async Task<bool> AdjustPrepaidBalanceAsync(string id, decimal delta, CancellationToken ct = default)
    {
        var affected = await _db.LicenseAgreements
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.PrepaidBalance, x => x.PrepaidBalance + delta)
                .SetProperty(x => x.LastUpdatedAtUtc, DateTime.UtcNow),
                ct);

        return affected > 0;
    }

    // ------------------------ DELETE ------------------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        // NEW: clear org ➜ license link if present
        await _db.Organizations
            .Where(o => o.LicenseAgreementId == id)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.LicenseAgreementId, (string?)null), ct);

        var entity = await _db.LicenseAgreements.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity is null) return false;

        _db.LicenseAgreements.Remove(entity);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
