using BlazorLoginDemo.Shared.Models.Kernel.User;
using BlazorLoginDemo.Shared.Services.Interfaces.User;
using Microsoft.EntityFrameworkCore;

namespace BlazorLoginDemo.Shared.Services.User;

public sealed class AvaUserSysPreferenceService : IAvaUserSysPreferenceService
{
    private readonly ApplicationDbContext _db;

    public AvaUserSysPreferenceService(ApplicationDbContext db) => _db = db;

    // CREATE
    public async Task<AvaUserSysPreference> CreateAsync(AvaUserSysPreference preference, CancellationToken ct = default)
    {
        if (preference is null) throw new ArgumentNullException(nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.AspNetUsersId))
            throw new ArgumentException("AspNetUsersId must be provided.", nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.Email))
            throw new ArgumentException("Email must be provided.", nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.FirstName))
            throw new ArgumentException("FirstName must be provided.", nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.LastName))
            throw new ArgumentException("LastName must be provided.", nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.DefaultFlightSeating))
            throw new ArgumentException("DefaultFlightSeating must be provided.", nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.MaxFlightSeating))
            throw new ArgumentException("MaxFlightSeating must be provided.", nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.DefaultCurrencyCode))
            throw new ArgumentException("DefaultCurrencyCode must be provided.", nameof(preference));

        if (string.IsNullOrWhiteSpace(preference.Id))
            preference.Id = NanoidDotNet.Nanoid.Generate();

        await _db.AvaUserSysPreferences.AddAsync(preference, ct);
        await _db.SaveChangesAsync(ct);

        // update the foreign key on the AvaUser object
        var usr = await _db.AvaUsers.FirstOrDefaultAsync(x => x.Id == preference.AvaUserId);
        if (usr is null) return preference;
        usr.AvaUserSysPreferenceId = preference.Id;
        await _db.SaveChangesAsync(ct);

        return preference;
    }

    // READ
    public async Task<AvaUserSysPreference?> GetByIdAsync(string id, CancellationToken ct = default)
        => await _db.AvaUserSysPreferences.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<AvaUserSysPreference?> GetByUserIdAsync(string avaUserId, CancellationToken ct = default)
        => await _db.AvaUserSysPreferences
            .AsNoTracking()
            .Where(x => x.AvaUserId == avaUserId)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<AvaUserSysPreference>> GetAllAsync(CancellationToken ct = default)
        => await _db.AvaUserSysPreferences.AsNoTracking()
            .OrderBy(x => x.Email).ToListAsync(ct);

    public async Task<IReadOnlyList<AvaUserSysPreference>> GetForUserAsync(string aspNetUserId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(aspNetUserId)) return Array.Empty<AvaUserSysPreference>();
        return await _db.AvaUserSysPreferences.AsNoTracking()
            .Where(x => x.AspNetUsersId == aspNetUserId)
            .ToListAsync(ct);
    }

    // UPDATE
    public async Task<AvaUserSysPreference> UpdateAsync(AvaUserSysPreference preference, CancellationToken ct = default)
    {
        if (preference is null) throw new ArgumentNullException(nameof(preference));
        if (string.IsNullOrWhiteSpace(preference.Id))
            throw new ArgumentException("Id must be provided for update.", nameof(preference));

        _db.Attach(preference);
        _db.Entry(preference).State = EntityState.Modified;
        await _db.SaveChangesAsync(ct);
        return preference;
    }

    // DELETE
    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var existing = await _db.AvaUserSysPreferences.FindAsync([id], ct);
        if (existing is null) return false;

        _db.AvaUserSysPreferences.Remove(existing);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    // UTIL
    public async Task<bool> ExistsAsync(string id, CancellationToken ct = default)
        => await _db.AvaUserSysPreferences.AsNoTracking().AnyAsync(x => x.Id == id, ct);
}
