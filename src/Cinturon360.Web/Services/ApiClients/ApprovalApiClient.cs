using Cinturon360.Contracts.Approvals;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class ApprovalApiClient : ApiClientBase
{
    public ApprovalApiClient(HttpClient http, ILogger<ApprovalApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<ApprovalDetailResponse>> GetApprovalAsync(string approvalId, CancellationToken ct = default)
        => GetAsync<ApprovalDetailResponse>($"api/v1/approvals/{approvalId}", ct);

    public async Task<ApiResult<PendingApprovalsResponse>> ListPendingAsync(string approverUserId, CancellationToken ct = default)
    {
        var result = await GetAsync<IReadOnlyList<ApprovalRequestResponse>>(
            $"api/v1/approvals/pending/{approverUserId}",
            ct);

        return result.IsSuccess && result.Value is not null
            ? ApiResult<PendingApprovalsResponse>.Ok(new PendingApprovalsResponse(result.Value))
            : ApiResult<PendingApprovalsResponse>.Fail(result.Error ?? "Unable to load approvals.");
    }

    public async Task<ApiResult<ApprovalHistoryResponse>> ListHistoryAsync(string userId, CancellationToken ct = default)
    {
        var result = await GetAsync<IReadOnlyList<ApprovalRequestResponse>>(
            $"api/v1/approvals/history/{userId}",
            ct);

        return result.IsSuccess && result.Value is not null
            ? ApiResult<ApprovalHistoryResponse>.Ok(new ApprovalHistoryResponse(result.Value))
            : ApiResult<ApprovalHistoryResponse>.Fail(result.Error ?? "Unable to load approval history.");
    }

    public Task<ApiResult<string>> SubmitForApprovalAsync(SubmitForApprovalRequest request, CancellationToken ct = default)
        => PostAsync<string>("api/v1/approvals", request, ct);

    public Task<ApiResult<bool>> ApproveAsync(string approvalId, ApproveRequest request, CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/approvals/{approvalId}/approve", request, ct);

    public Task<ApiResult<bool>> RejectAsync(string approvalId, RejectRequest request, CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/approvals/{approvalId}/reject", request, ct);
}
