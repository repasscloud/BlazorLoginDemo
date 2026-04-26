using Cinturon360.Contracts.Travellers;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class TravellerApiClient : ApiClientBase
{
    public TravellerApiClient(HttpClient http, ILogger<TravellerApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<TravellerProfileResponse>> GetProfileAsync(string userId, CancellationToken ct = default)
        => GetAsync<TravellerProfileResponse>($"api/v1/travellers/{userId}/profile", ct);

    public Task<ApiResult<TravellerProfileResponse>> UpdateProfileAsync(string userId, UpdateTravellerProfileRequest request, CancellationToken ct = default)
        => PutAsync<TravellerProfileResponse>($"api/v1/travellers/{userId}/profile", request, ct);

    public Task<ApiResult<LoyaltyProgramsResponse>> GetLoyaltyProgramsAsync(string userId, CancellationToken ct = default)
        => GetAsync<LoyaltyProgramsResponse>($"api/v1/travellers/{userId}/loyalty", ct);

    public Task<ApiResult<LoyaltyProgramsResponse>> AddLoyaltyProgramAsync(string userId, AddLoyaltyProgramRequest request, CancellationToken ct = default)
        => PostAsync<LoyaltyProgramsResponse>($"api/v1/travellers/{userId}/loyalty", request, ct);

    public Task<ApiResult<EmergencyContactsResponse>> GetEmergencyContactsAsync(string userId, CancellationToken ct = default)
        => GetAsync<EmergencyContactsResponse>($"api/v1/travellers/{userId}/emergency-contacts", ct);

    public Task<ApiResult<EmergencyContactsResponse>> AddEmergencyContactAsync(string userId, AddEmergencyContactRequest request, CancellationToken ct = default)
        => PostAsync<EmergencyContactsResponse>($"api/v1/travellers/{userId}/emergency-contacts", request, ct);

    public Task<ApiResult<TravelPreferencesResponse>> GetPreferencesAsync(string userId, CancellationToken ct = default)
        => GetAsync<TravelPreferencesResponse>($"api/v1/travellers/{userId}/preferences", ct);

    public Task<ApiResult<TravelPreferencesResponse>> UpdatePreferencesAsync(string userId, UpdateTravelPreferencesRequest request, CancellationToken ct = default)
        => PutAsync<TravelPreferencesResponse>($"api/v1/travellers/{userId}/preferences", request, ct);
}
