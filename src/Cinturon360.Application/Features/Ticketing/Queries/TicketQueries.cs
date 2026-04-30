using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Ticketing;
using Cinturon360.Domain.Entities.Ticketing;
using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Application.Features.Ticketing.Queries;

// ── Get ticket detail ──────────────────────────────────────────────────────
public sealed record GetTicketDetailQuery(
    string TicketId,
    bool   CallerIsSupport) : IRequest<Result<TicketDetailResponse?>>;

public sealed class GetTicketDetailHandler : IRequestHandler<GetTicketDetailQuery, Result<TicketDetailResponse?>>
{
    private readonly ISupportTicketRepository _tickets;

    public GetTicketDetailHandler(ISupportTicketRepository tickets) => _tickets = tickets;

    public async Task<Result<TicketDetailResponse?>> Handle(GetTicketDetailQuery request, CancellationToken ct)
    {
        var ticket = await _tickets.GetDetailAsync(request.TicketId, request.CallerIsSupport, ct);
        if (ticket is null) return Result.Success<TicketDetailResponse?>(null);

        return Result.Success<TicketDetailResponse?>(MapDetail(ticket, request.CallerIsSupport));
    }

    private static TicketDetailResponse MapDetail(SupportTicket t, bool isSupport)
        => new(
            MapSummary(t),
            t.Comments
                .Where(c => isSupport || !c.IsPrivate)
                .Select(MapComment)
                .ToList(),
            t.Escalations
                .Select(MapEscalation)
                .ToList(),
            t.Attachments
                .Where(a => isSupport || !a.IsPrivate)
                .Select(MapAttachment)
                .ToList());

    internal static TicketSummaryResponse MapSummary(SupportTicket t)
        => new(t.Id, t.RaisedByUserId, t.EmailMeUpdates, t.Subject, t.Status, t.Priority, t.Queue, t.Category,
               t.GitHubIssueNumber, t.GitHubIssueUrl, t.CreatedAt, t.UpdatedAt);

    internal static TicketCommentResponse MapComment(TicketComment c)
        => new(c.Id, c.AuthorDisplayName, c.Body, c.IsPrivate, c.CreatedAt);

    internal static TicketEscalationResponse MapEscalation(TicketEscalation e)
        => new(e.Id, e.FromQueue, e.ToQueue, e.IsEscalation, e.Reason, e.CreatedAt);

    internal static TicketAttachmentResponse MapAttachment(TicketAttachment a)
        => new(a.Id, a.FileName, a.ContentType, a.FileSize, a.IsPrivate, a.CreatedAt);
}

// ── List tickets for a user ────────────────────────────────────────────────
public sealed record ListMyTicketsQuery(
    string UserId,
    int    Page,
    int    PageSize) : IRequest<Result<TicketListResponse>>;

public sealed class ListMyTicketsHandler : IRequestHandler<ListMyTicketsQuery, Result<TicketListResponse>>
{
    private readonly ISupportTicketRepository _tickets;

    public ListMyTicketsHandler(ISupportTicketRepository tickets) => _tickets = tickets;

    public async Task<Result<TicketListResponse>> Handle(ListMyTicketsQuery request, CancellationToken ct)
    {
        var (items, total) = await _tickets.ListByUserAsync(request.UserId, request.Page, request.PageSize, ct);
        return Result.Success(new TicketListResponse(
            items.Select(GetTicketDetailHandler.MapSummary).ToList(),
            total));
    }
}

// ── List tickets for org (support dashboard) ──────────────────────────────
public sealed record ListOrgTicketsQuery(
    string         OrgId,
    TicketStatus?  Status,
    TicketQueue?   Queue,
    int            Page,
    int            PageSize) : IRequest<Result<TicketListResponse>>;

public sealed class ListOrgTicketsHandler : IRequestHandler<ListOrgTicketsQuery, Result<TicketListResponse>>
{
    private readonly ISupportTicketRepository _tickets;

    public ListOrgTicketsHandler(ISupportTicketRepository tickets) => _tickets = tickets;

    public async Task<Result<TicketListResponse>> Handle(ListOrgTicketsQuery request, CancellationToken ct)
    {
        var (items, total) = await _tickets.ListByOrgAsync(
            request.OrgId, request.Status, request.Queue,
            request.Page, request.PageSize, ct);

        return Result.Success(new TicketListResponse(
            items.Select(GetTicketDetailHandler.MapSummary).ToList(),
            total));
    }
}

// ── Download attachment ────────────────────────────────────────────────────
/// <summary>
/// Returns the raw attachment (with content bytes) or null if not found / caller not authorised.
/// Callers must pre-authorise: owner of the ticket can access non-private attachments;
/// support can access all. The API endpoint enforces this before sending the query.
/// </summary>
public sealed record DownloadAttachmentQuery(
    string AttachmentId) : IRequest<Result<TicketAttachment?>>;

public sealed class DownloadAttachmentHandler : IRequestHandler<DownloadAttachmentQuery, Result<TicketAttachment?>>
{
    private readonly ISupportTicketRepository _tickets;
    public DownloadAttachmentHandler(ISupportTicketRepository tickets) => _tickets = tickets;

    public async Task<Result<TicketAttachment?>> Handle(DownloadAttachmentQuery request, CancellationToken ct)
    {
        var attachment = await _tickets.GetAttachmentAsync(request.AttachmentId, ct);
        return Result.Success<TicketAttachment?>(attachment);
    }
}

public sealed record GetTicketEmailTemplateQuery(string TemplateId)
    : IRequest<Result<TicketEmailTemplateDetailResponse?>>;

public sealed class GetTicketEmailTemplateHandler : IRequestHandler<GetTicketEmailTemplateQuery, Result<TicketEmailTemplateDetailResponse?>>
{
    private readonly ITicketEmailTemplateRepository _templates;

    public GetTicketEmailTemplateHandler(ITicketEmailTemplateRepository templates) => _templates = templates;

    public async Task<Result<TicketEmailTemplateDetailResponse?>> Handle(GetTicketEmailTemplateQuery request, CancellationToken ct)
    {
        var template = await _templates.GetByIdAsync(request.TemplateId, ct);
        if (template is null)
            return Result.Success<TicketEmailTemplateDetailResponse?>(null);

        return Result.Success<TicketEmailTemplateDetailResponse?>(MapDetail(template));
    }

    internal static TicketEmailTemplateDetailResponse MapDetail(TicketEmailTemplate template)
        => new(
            template.Id,
            template.Code,
            template.LanguageCode,
            template.HtmlBody,
            template.PlainTextBody,
            template.Description,
            template.IsActive,
            template.CreatedAt,
            template.UpdatedAt);
}

public sealed record ListTicketEmailTemplatesQuery(string? Code = null)
    : IRequest<Result<IReadOnlyList<TicketEmailTemplateResponse>>>;

public sealed class ListTicketEmailTemplatesHandler : IRequestHandler<ListTicketEmailTemplatesQuery, Result<IReadOnlyList<TicketEmailTemplateResponse>>>
{
    private readonly ITicketEmailTemplateRepository _templates;

    public ListTicketEmailTemplatesHandler(ITicketEmailTemplateRepository templates) => _templates = templates;

    public async Task<Result<IReadOnlyList<TicketEmailTemplateResponse>>> Handle(ListTicketEmailTemplatesQuery request, CancellationToken ct)
    {
        var items = await _templates.ListAsync(request.Code, ct);
        return Result.Success<IReadOnlyList<TicketEmailTemplateResponse>>(
            items.Select(x => new TicketEmailTemplateResponse(
                x.Id,
                x.Code,
                x.LanguageCode,
                x.Description,
                x.IsActive,
                x.UpdatedAt)).ToList());
    }
}
