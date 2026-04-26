using Cinturon360.Contracts.Approvals;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class ApprovalApiClient : ApiClientBase
{
    public ApprovalApiClient(HttpClient http, ILogger<ApprovalApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<ApprovalRequestResponse>> GetApprovalAsync(string approvalId, CancellationToken ct = default)
        => GetAsync<ApprovalRequestResponse>($"api/v1/approvals/{approvalId}", ct);

    public Task<ApiResult<PendingApprovalsResponse>> ListPendingAsync(CancellationToken ct = default)
        => GetAsync<PendingApprovalsResponse>("api/v1/approvals/pending", ct);

    public Task<ApiResult<ApprovalRequestResponse>> SubmitForApprovalAsync(SubmitForApprovalRequest request, CancellationToken ct = default)
        => PostAsync<ApprovalRequestResponse>("api/v1/approvals", request, ct);

    public Task<ApiResult<ApprovalRequestResponse>> ApproveAsync(string approvalId, ApproveRequest request, CancellationToken ct = default)
        => PostAsync<ApprovalRequestResponse>($"api/v1/approvals/{approvalId}/approve", request, ct);

    public Task<ApiResult<ApprovalRequestResponse>> RejectAsync(string approvalId, RejectRequest request, CancellationToken ct = default)
        => PostAsync<ApprovalRequestResponse>($"api/v1/approvals/{approvalId}/reject", request, ct);
}
