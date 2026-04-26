using Cinturon360.Contracts.Policies;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class PolicyApiClient : ApiClientBase
{
    public PolicyApiClient(HttpClient http, ILogger<PolicyApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<TravelPolicyResponse>> GetPolicyAsync(string orgId, string policyId, CancellationToken ct = default)
        => GetAsync<TravelPolicyResponse>($"api/v1/organisations/{orgId}/policies/{policyId}", ct);

    public Task<ApiResult<PolicyListResponse>> ListPoliciesAsync(string orgId, CancellationToken ct = default)
        => GetAsync<PolicyListResponse>($"api/v1/organisations/{orgId}/policies", ct);

    public Task<ApiResult<TravelPolicyResponse>> CreatePolicyAsync(string orgId, CreateTravelPolicyRequest request, CancellationToken ct = default)
        => PostAsync<TravelPolicyResponse>($"api/v1/organisations/{orgId}/policies", request, ct);

    public Task<ApiResult<TravelPolicyResponse>> AddRuleAsync(string orgId, string policyId, AddPolicyRuleRequest request, CancellationToken ct = default)
        => PostAsync<TravelPolicyResponse>($"api/v1/organisations/{orgId}/policies/{policyId}/rules", request, ct);

    public Task<ApiResult<bool>> AssignPolicyAsync(string orgId, string policyId, AssignPolicyRequest request, CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/organisations/{orgId}/policies/{policyId}/assign", request, ct);
}
