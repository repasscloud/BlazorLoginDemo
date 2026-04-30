using Cinturon360.Contracts.System;
using Cinturon360.Contracts.Ticketing;

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

    public Task<ApiResult<IReadOnlyList<TicketEmailTemplateResponse>>> ListTicketEmailTemplatesAsync(string? code = null, CancellationToken ct = default)
    {
        var path = string.IsNullOrWhiteSpace(code)
            ? "api/v1/system/ticket-email-templates"
            : $"api/v1/system/ticket-email-templates?code={Uri.EscapeDataString(code)}";

        return GetAsync<IReadOnlyList<TicketEmailTemplateResponse>>(path, ct);
    }

    public Task<ApiResult<TicketEmailTemplateDetailResponse>> GetTicketEmailTemplateAsync(string templateId, CancellationToken ct = default)
        => GetAsync<TicketEmailTemplateDetailResponse>($"api/v1/system/ticket-email-templates/{Uri.EscapeDataString(templateId)}", ct);

    public Task<ApiResult<bool>> DeleteTicketEmailTemplateAsync(string templateId, CancellationToken ct = default)
        => DeleteAsync($"api/v1/system/ticket-email-templates/{Uri.EscapeDataString(templateId)}", ct);
}
