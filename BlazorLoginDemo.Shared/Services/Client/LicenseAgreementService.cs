using BlazorLoginDemo.Shared.Models.Kernel.Billing;
using BlazorLoginDemo.Shared.Services.Interfaces.Client;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.Client;

public sealed class LicenseAgreementService : ILicenseAgreementService
{
    private readonly ApplicationDbContext _db;

    public LicenseAgreementService(ApplicationDbContext db) => _db = db;

    // -----------------------------
    // CREATE
    // -----------------------------
    public async Task<LicenseAgreement> CreateAsync(LicenseAgreement agreement, CancellationToken ct = default)
    {
        if (agreement is null) throw new ArgumentNullException(nameof(agreement));
        if (string.IsNullOrWhiteSpace(agreement.AvaClientId))
            throw new ArgumentException("AvaClientId must be provided.", nameof(agreement));

        // Ensure ID and timestamps
        if (string.IsNullOrWhiteSpace(agreement.Id))
            agreement.Id = NanoidDotNet.Nanoid.Generate(NanoidDotNet.Nanoid.Alphabets.HexadecimalUppercase, 14);

        agreement.CreatedAt = DateTime.UtcNow;
        agreement.LastUpdatedAt = DateTime.UtcNow;

        await _db.LicenseAgreements.AddAsync(agreement, ct);

        var client = await _db.AvaClients.FirstOrDefaultAsync(c => c.Id == agreement.AvaClientId, ct);
        if (client is not null)
        {
            client.LicenseAgreementId = agreement.Id;
            client.LastUpdated = DateTime.UtcNow;
        }

        var lateFeeConfig = await _db.LateFeeConfigs.FirstOrDefaultAsync(c => c.Id == agreement.LateFeeConfigId, ct);
        if (lateFeeConfig is null)
        {
            if (!string.IsNullOrEmpty(agreement.LateFeeConfigId))
            {
                lateFeeConfig = new LateFeeConfig
                {
                    Id = agreement.LateFeeConfigId,
                    LicenseAgreementId = agreement.Id,
                };

                await _db.LateFeeConfigs.AddAsync(lateFeeConfig, ct);
            }
        }
        await _db.SaveChangesAsync(ct);
        return agreement;
    }

    // -----------------------------
    // READ
    // -----------------------------
    public async Task<LicenseAgreement?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.LicenseAgreements.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LicenseAgreement>> GetAllAsync(CancellationToken ct = default)
        => await _db.LicenseAgreements.AsNoTracking()
            .OrderBy(x => x.AvaClientId)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<LicenseAgreement>> GetForClientAsync(string clientId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(clientId)) return Array.Empty<LicenseAgreement>();
        return await _db.LicenseAgreements.AsNoTracking()
            .Where(x => x.AvaClientId == clientId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    // -----------------------------
    // UPDATE
    // -----------------------------
    public async Task<LicenseAgreement> UpdateAsync(LicenseAgreement agreement, CancellationToken ct = default)
    {
        if (agreement is null) throw new ArgumentNullException(nameof(agreement));
        if (string.IsNullOrWhiteSpace(agreement.Id))
            throw new ArgumentException("Id must be provided for update.", nameof(agreement));

        agreement.LastUpdatedAt = DateTime.UtcNow;

        _db.Attach(agreement);
        _db.Entry(agreement).State = EntityState.Modified;

        await _db.SaveChangesAsync(ct);
        return agreement;
    }

    // -----------------------------
    // DELETE
    // -----------------------------
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _db.LicenseAgreements.FindAsync([id], ct);
        if (existing is null) return false;

        _db.LicenseAgreements.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // -----------------------------
    // UTIL
    // -----------------------------
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.LicenseAgreements.AsNoTracking().AnyAsync(x => x.Id == id, ct);
}
