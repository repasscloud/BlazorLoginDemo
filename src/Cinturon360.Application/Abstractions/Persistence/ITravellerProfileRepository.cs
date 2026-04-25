using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Application.Abstractions.Persistence;

public interface ITravellerProfileRepository
{
    Task<TravellerProfile?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<TravellerProfile?> GetByIdAsync(string id, CancellationToken ct = default);
    Task AddAsync(TravellerProfile profile, CancellationToken ct = default);
    void Update(TravellerProfile profile);

    Task<IReadOnlyList<TravellerLoyaltyProgram>> GetLoyaltyProgramsAsync(string userId, CancellationToken ct = default);
    Task AddLoyaltyProgramAsync(TravellerLoyaltyProgram program, CancellationToken ct = default);
    void RemoveLoyaltyProgram(TravellerLoyaltyProgram program);

    Task<IReadOnlyList<UserEmergencyContact>> GetEmergencyContactsAsync(string userId, CancellationToken ct = default);
    Task AddEmergencyContactAsync(UserEmergencyContact contact, CancellationToken ct = default);
    void UpdateEmergencyContact(UserEmergencyContact contact);
    void RemoveEmergencyContact(UserEmergencyContact contact);

    Task<IReadOnlyList<UserAddress>> GetAddressesAsync(string userId, CancellationToken ct = default);
    Task AddAddressAsync(UserAddress address, CancellationToken ct = default);
    void UpdateAddress(UserAddress address);
    void RemoveAddress(UserAddress address);

    Task<UserPreferences?> GetPreferencesAsync(string userId, CancellationToken ct = default);
    Task AddPreferencesAsync(UserPreferences preferences, CancellationToken ct = default);
    void UpdatePreferences(UserPreferences preferences);
}
