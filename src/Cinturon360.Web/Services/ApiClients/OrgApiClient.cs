using Cinturon360.Contracts.Organizations;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class OrgApiClient : ApiClientBase
{
    public OrgApiClient(HttpClient http, ILogger<OrgApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<OrganisationResponse>> GetOrgAsync(string orgId, CancellationToken ct = default)
        => GetAsync<OrganisationResponse>($"api/v1/organisations/{orgId}", ct);

    public Task<ApiResult<OrganisationListResponse>> ListOrgsAsync(CancellationToken ct = default)
        => GetAsync<OrganisationListResponse>("api/v1/organisations", ct);

    public Task<ApiResult<OrganisationResponse>> CreateOrgAsync(CreateOrganisationRequest request, CancellationToken ct = default)
        => PostAsync<OrganisationResponse>("api/v1/organisations", request, ct);

    public Task<ApiResult<OrganisationResponse>> UpdateOrgAsync(string orgId, UpdateOrganisationRequest request, CancellationToken ct = default)
        => PutAsync<OrganisationResponse>($"api/v1/organisations/{orgId}", request, ct);
}
