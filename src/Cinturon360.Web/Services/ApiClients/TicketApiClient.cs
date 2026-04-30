using Cinturon360.Contracts.Ticketing;
using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Web.Services.ApiClients;

public sealed class TicketApiClient : ApiClientBase
{
    public TicketApiClient(HttpClient http, ILogger<TicketApiClient> logger)
        : base(http, logger) { }

    public Task<ApiResult<TicketListResponse>> ListMyTicketsAsync(
        string userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
        => GetAsync<TicketListResponse>(
            $"api/v1/tickets/my?userId={Uri.EscapeDataString(userId)}&page={page}&pageSize={pageSize}", ct);

    public Task<ApiResult<TicketDetailResponse>> GetDetailAsync(
        string ticketId, bool isSupport, CancellationToken ct = default)
        => GetAsync<TicketDetailResponse>(
            $"api/v1/tickets/{ticketId}?isSupport={isSupport}", ct);

    public Task<ApiResult<string>> RaiseTicketAsync(
        string orgId,
        string userId,
        string firstName,
        string? orgSupportTeamName,
        TicketCategory category,
        TicketPriority priority,
        string subject,
        string description,
        string? errorContext,
        CancellationToken ct = default)
        => PostAsync<string>("api/v1/tickets", new
        {
            OrgId              = orgId,
            RaisedByUserId     = userId,
            RaisedByFirstName  = firstName,
            OrgSupportTeamName = orgSupportTeamName,
            Category           = (int)category,
            Priority           = (int)priority,
            Subject            = subject,
            Description        = description,
            ErrorContext       = errorContext
        }, ct);

    public Task<ApiResult<string>> AddCommentAsync(
        string ticketId,
        string authorUserId,
        string authorDisplayName,
        bool callerIsSupport,
        string body,
        bool isPrivate,
        CancellationToken ct = default)
        => PostAsync<string>($"api/v1/tickets/{ticketId}/comments", new
        {
            AuthorUserId      = authorUserId,
            AuthorDisplayName = authorDisplayName,
            CallerIsSupport   = callerIsSupport,
            Body              = body,
            IsPrivate         = isPrivate
        }, ct);

    public Task<ApiResult<bool>> CloseTicketAsync(
        string ticketId,
        string actorUserId,
        string actorDisplayName,
        string? resolutionNote,
        CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/tickets/{ticketId}/close", new
        {
            ActorUserId      = actorUserId,
            ActorDisplayName = actorDisplayName,
            ResolutionNote   = resolutionNote
        }, ct);

    public Task<ApiResult<TicketListResponse>> ListOrgTicketsAsync(
        string orgId,
        TicketStatus? status = null,
        TicketQueue?  queue  = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken ct = default)
    {
        var url = $"api/v1/tickets/org/{orgId}?page={page}&pageSize={pageSize}";
        if (status.HasValue) url += $"&status={(int)status.Value}";
        if (queue.HasValue)  url += $"&queue={(int)queue.Value}";
        return GetAsync<TicketListResponse>(url, ct);
    }

    public Task<ApiResult<bool>> EscalateAsync(
        string ticketId,
        string actorUserId,
        string actorDisplayName,
        TicketQueue toQueue,
        TicketPriority? newPriority,
        string? reason,
        CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/tickets/{ticketId}/escalate", new
        {
            ActorUserId      = actorUserId,
            ActorDisplayName = actorDisplayName,
            ToQueue          = (int)toQueue,
            NewPriority      = newPriority.HasValue ? (int?)newPriority.Value : null,
            Reason           = reason
        }, ct);

    public Task<ApiResult<bool>> DeescalateAsync(
        string ticketId,
        string actorUserId,
        string actorDisplayName,
        TicketQueue toQueue,
        string? reason,
        CancellationToken ct = default)
        => PostAsync<bool>($"api/v1/tickets/{ticketId}/deescalate", new
        {
            ActorUserId      = actorUserId,
            ActorDisplayName = actorDisplayName,
            ToQueue          = (int)toQueue,
            Reason           = reason
        }, ct);

    public async Task<ApiResult<string>> UploadAttachmentAsync(
        string ticketId,
        string uploaderUserId,
        bool   uploaderIsSupport,
        bool   isPrivate,
        string fileName,
        string contentType,
        byte[] content,
        CancellationToken ct = default)
    {
        var url = $"api/v1/tickets/{ticketId}/attachments"
                + $"?uploaderUserId={Uri.EscapeDataString(uploaderUserId)}"
                + $"&uploaderIsSupport={uploaderIsSupport}"
                + $"&isPrivate={isPrivate}";

        using var form = new MultipartFormDataContent();
        using var fileContent = new ByteArrayContent(content);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(fileContent, "file", fileName);

        return await PostMultipartAsync<string>(url, form, ct);
    }

    /// <summary>Returns the URL to use in an anchor/img tag for downloading an attachment.</summary>
    public string GetAttachmentDownloadUrl(
        string ticketId, string attachmentId, string callerUserId, bool callerIsSupport)
        => $"api/v1/tickets/{ticketId}/attachments/{attachmentId}"
         + $"?callerUserId={Uri.EscapeDataString(callerUserId)}&callerIsSupport={callerIsSupport}";
}

