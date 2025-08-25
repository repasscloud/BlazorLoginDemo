using BlazorLoginDemo.Shared.Services.Interfaces.Client;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Client;

public sealed class AvaClientLicenseService : IAvaClientLicenseService
{
    private readonly ApplicationDbContext _db;

    public AvaClientLicenseService(ApplicationDbContext db) => _db = db;

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<AvaClientLicense> CreateAsync(AvaClientLicense license, CancellationToken ct = default)
    {
        // Enforce required values at service boundary
        if (license is null) throw new ArgumentNullException(nameof(license));

        if (string.IsNullOrWhiteSpace(license.ClientID))
            throw new ArgumentException("ClientID must be provided.", nameof(license));

        if (string.IsNullOrWhiteSpace(license.AppID))
            throw new ArgumentException("AppID must be provided.", nameof(license));

        if (string.IsNullOrWhiteSpace(license.IssuedBy))
            throw new ArgumentException("IssuedBy must be provided.", nameof(license));

        if (license.ExpiryDate == default)
            throw new ArgumentException("ExpiryDate must be provided.", nameof(license));

        // SpendThreshold is [Required] in the model; ensure a sensible check
        // (adjust if zero is valid in your domain)
        if (license.SpendThreshold <= 0)
            throw new ArgumentException("SpendThreshold must be greater than zero.", nameof(license));

        // Ensure timestamps/ids are set if caller left them default
        if (string.IsNullOrWhiteSpace(license.Id))
            license.Id = NanoidDotNet.Nanoid.Generate();

        if (license.GeneratedOn == default)
            license.GeneratedOn = DateTime.UtcNow;

        await _db.AvaClientLicenses.AddAsync(license, ct);
        await _db.SaveChangesAsync(ct);
        return license;
    }

    // -----------------------------
    // READ
    // -----------------------------
    public async Task<AvaClientLicense?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.AvaClientLicenses.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<AvaClientLicense>> GetAllAsync(CancellationToken ct = default)
        => await _db.AvaClientLicenses.AsNoTracking()
            .OrderBy(x => x.ClientID).ThenBy(x => x.AppID).ThenByDescending(x => x.GeneratedOn)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<AvaClientLicense>> GetForClientAsync(string clientId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(clientId)) return Array.Empty<AvaClientLicense>();
        return await _db.AvaClientLicenses.AsNoTracking()
            .Where(x => x.ClientID == clientId)
            .OrderByDescending(x => x.GeneratedOn)
            .ToListAsync(ct);
    }

    // -----------------------------
    // UPDATE (replace whole object)
    // -----------------------------
    public async Task<AvaClientLicense> UpdateAsync(AvaClientLicense license, CancellationToken ct = default)
    {
        if (license is null) throw new ArgumentNullException(nameof(license));
        if (string.IsNullOrWhiteSpace(license.Id))
            throw new ArgumentException("Id must be provided for update.", nameof(license));

        // Same required-field guarantees on update (you said required must be present)
        if (string.IsNullOrWhiteSpace(license.ClientID))
            throw new ArgumentException("ClientID must be provided.", nameof(license));
        if (string.IsNullOrWhiteSpace(license.AppID))
            throw new ArgumentException("AppID must be provided.", nameof(license));
        if (string.IsNullOrWhiteSpace(license.IssuedBy))
            throw new ArgumentException("IssuedBy must be provided.", nameof(license));
        if (license.ExpiryDate == default)
            throw new ArgumentException("ExpiryDate must be provided.", nameof(license));
        if (license.SpendThreshold <= 0)
            throw new ArgumentException("SpendThreshold must be greater than zero.", nameof(license));

        _db.Attach(license);
        _db.Entry(license).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return license;
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _db.AvaClientLicenses.FindAsync([id], ct);
        if (existing is null) return false;

        _db.AvaClientLicenses.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // UTIL
    // -----------------------------
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.AvaClientLicenses.AsNoTracking().AnyAsync(x => x.Id == id, ct);
}