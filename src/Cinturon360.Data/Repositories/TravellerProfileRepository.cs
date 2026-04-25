using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Data.Repositories;

internal sealed class TravellerProfileRepository : ITravellerProfileRepository
{
    private readonly AppDbContext _db;
    public TravellerProfileRepository(AppDbContext db) => _db = db;

    public Task<TravellerProfile?> GetByIdAsync(string id, CancellationToken ct)
        => _db.TravellerProfiles.FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<TravellerProfile?> GetByUserIdAsync(string userId, CancellationToken ct)
        => _db.TravellerProfiles.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task AddAsync(TravellerProfile profile, CancellationToken ct)
        => await _db.TravellerProfiles.AddAsync(profile, ct);

    public void Update(TravellerProfile profile)
        => _db.TravellerProfiles.Update(profile);

    // ── Loyalty programs ─────────────────────────────────────────────────
    public async Task<IReadOnlyList<TravellerLoyaltyProgram>> GetLoyaltyProgramsAsync(string userId, CancellationToken ct)
        => await _db.TravellerLoyaltyPrograms.Where(x => x.UserId == userId).ToListAsync(ct);

    public async Task AddLoyaltyProgramAsync(TravellerLoyaltyProgram program, CancellationToken ct)
        => await _db.TravellerLoyaltyPrograms.AddAsync(program, ct);

    public void RemoveLoyaltyProgram(TravellerLoyaltyProgram program)
        => _db.TravellerLoyaltyPrograms.Remove(program);

    // ── Emergency contacts ────────────────────────────────────────────────
    public async Task<IReadOnlyList<UserEmergencyContact>> GetEmergencyContactsAsync(string userId, CancellationToken ct)
        => await _db.UserEmergencyContacts.Where(x => x.UserId == userId).ToListAsync(ct);

    public async Task AddEmergencyContactAsync(UserEmergencyContact contact, CancellationToken ct)
        => await _db.UserEmergencyContacts.AddAsync(contact, ct);

    public void UpdateEmergencyContact(UserEmergencyContact contact)
        => _db.UserEmergencyContacts.Update(contact);

    public void RemoveEmergencyContact(UserEmergencyContact contact)
        => _db.UserEmergencyContacts.Remove(contact);

    // ── Addresses ─────────────────────────────────────────────────────────
    public async Task<IReadOnlyList<UserAddress>> GetAddressesAsync(string userId, CancellationToken ct)
        => await _db.UserAddresses.Where(x => x.UserId == userId).ToListAsync(ct);

    public async Task AddAddressAsync(UserAddress address, CancellationToken ct)
        => await _db.UserAddresses.AddAsync(address, ct);

    public void UpdateAddress(UserAddress address)
        => _db.UserAddresses.Update(address);

    public void RemoveAddress(UserAddress address)
        => _db.UserAddresses.Remove(address);

    // ── Preferences ───────────────────────────────────────────────────────
    public Task<UserPreferences?> GetPreferencesAsync(string userId, CancellationToken ct)
        => _db.UserPreferences.FirstOrDefaultAsync(x => x.UserId == userId, ct);

    public async Task AddPreferencesAsync(UserPreferences preferences, CancellationToken ct)
        => await _db.UserPreferences.AddAsync(preferences, ct);

    public void UpdatePreferences(UserPreferences preferences)
        => _db.UserPreferences.Update(preferences);
}
