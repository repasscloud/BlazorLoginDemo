using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using BlazorLoginDemo.Shared.Services.Interfaces.Finance;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Finance;

public sealed class LateFeeConfigService : ILateFeeConfigService
{
    private readonly ApplicationDbContext _db;

    public LateFeeConfigService(ApplicationDbContext db) => _db = db;

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<LateFeeConfig> CreateAsync(LateFeeConfig config, CancellationToken ct = default)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));

        if (string.IsNullOrWhiteSpace(config.LicenseAgreementId))
            throw new ArgumentException("LicenseAgreementId must be provided.", nameof(config));

        if (string.IsNullOrWhiteSpace(config.Id))
            config.Id = NanoidDotNet.Nanoid.Generate(NanoidDotNet.Nanoid.Alphabets.UppercaseLettersAndDigits, 14);

        await _db.LateFeeConfigs.AddAsync(config, ct);
        await _db.SaveChangesAsync(ct);
        return config;
    }

    // -----------------------------
    // READ
    // -----------------------------
    public async Task<LateFeeConfig?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.LateFeeConfigs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LateFeeConfig>> GetAllAsync(CancellationToken ct = default)
        => await _db.LateFeeConfigs.AsNoTracking()
            .OrderBy(x => x.LicenseAgreementId)
            .ToListAsync(ct);

    public async Task<LateFeeConfig?> GetForLicenseAgreementAsync(string licenseAgreementId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(licenseAgreementId)) return null;
        return await _db.LateFeeConfigs.AsNoTracking()
            .FirstOrDefaultAsync(x => x.LicenseAgreementId == licenseAgreementId, ct);
    }

    // -----------------------------
    // UPDATE
    // -----------------------------
    public async Task<LateFeeConfig> UpdateAsync(LateFeeConfig config, CancellationToken ct = default)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(config.Id))
            throw new ArgumentException("Id must be provided for update.", nameof(config));

        if (string.IsNullOrWhiteSpace(config.LicenseAgreementId))
            throw new ArgumentException("LicenseAgreementId must be provided.", nameof(config));

        _db.Attach(config);
        _db.Entry(config).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return config;
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _db.LateFeeConfigs.FindAsync([id], ct);
        if (existing is null) return false;

        _db.LateFeeConfigs.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // UTIL
    // -----------------------------
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.LateFeeConfigs.AsNoTracking().AnyAsync(x => x.Id == id, ct);
}
