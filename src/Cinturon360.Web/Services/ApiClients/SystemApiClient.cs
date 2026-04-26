using Cinturon360.Contracts.System;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class SystemApiClient : ApiClientBase
{
    public SystemApiClient(HttpClient http, ILogger<SystemApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<JobStatusResponse>> GetJobStatusAsync(string jobId, CancellationToken ct = default)
        => GetAsync<JobStatusResponse>($"api/v1/system/jobs/{jobId}", ct);

    public Task<ApiResult<JobListResponse>> ListJobsAsync(int page = 1, int pageSize = 25, CancellationToken ct = default)
        => GetAsync<JobListResponse>($"api/v1/system/jobs?page={page}&pageSize={pageSize}", ct);

    public Task<ApiResult<DocumentListResponse>> ListDocumentsAsync(CancellationToken ct = default)
        => GetAsync<DocumentListResponse>("api/v1/system/jobs/documents", ct);

    public Task<ApiResult<EnqueueJobResponse>> EnqueueJobAsync(EnqueueJobRequest request, CancellationToken ct = default)
        => PostAsync<EnqueueJobResponse>("api/v1/system/jobs", request, ct);
}
