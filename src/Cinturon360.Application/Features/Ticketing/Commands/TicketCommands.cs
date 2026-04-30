using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Ticketing;
using Cinturon360.Domain.Entities.Ticketing;
using Cinturon360.Domain.Enums.Ticketing;
using Cinturon360.Integrations.GitHub.Services;

namespace Cinturon360.Application.Features.Ticketing.Commands;

// ── Errors ──────────────────────────────────────────────────────────────────
public static class TicketErrors
{
    public static readonly Error NotFound    = new("ticket.not_found",    "Support ticket not found.");
    public static readonly Error AlreadyClosed = new("ticket.already_closed", "Ticket is closed and cannot be modified.");
    public static readonly Error Forbidden   = new("ticket.forbidden",    "You do not have permission to perform this action.");
}

// ── Raise Ticket ─────────────────────────────────────────────────────────────
public sealed record RaiseTicketCommand(
    string         OrgId,
    string         RaisedByUserId,
    string         RaisedByFirstName,   // for display name on system comment
    string?        OrgSupportTeamName,  // from org; falls back to "Support Team"
    TicketCategory Category,
    TicketPriority Priority,
    string         Subject,
    string         Description,
    string?        ErrorContext) : IRequest<Result<string>>;

public sealed class RaiseTicketHandler : IRequestHandler<RaiseTicketCommand, Result<string>>
{
    private readonly ISupportTicketRepository _tickets;
    private readonly IGitHubTicketingService  _github;
    private readonly IUnitOfWork              _uow;
    private readonly ILogger<RaiseTicketHandler> _logger;

    public RaiseTicketHandler(
        ISupportTicketRepository tickets,
        IGitHubTicketingService github,
        IUnitOfWork uow,
        ILogger<RaiseTicketHandler> logger)
    {
        _tickets = tickets;
        _github  = github;
        _uow     = uow;
        _logger  = logger;
    }

    public async Task<Result<string>> Handle(RaiseTicketCommand request, CancellationToken ct)
    {
        var id     = IdGenerator.New(IdPrefix.SupportTicket);
        var ticket = SupportTicket.Create(
            id,
            request.OrgId,
            request.RaisedByUserId,
            request.Category,
            request.Priority,
            request.Subject,
            request.Description,
            request.ErrorContext);

        await _tickets.AddAsync(ticket, ct);
        await _uow.SaveChangesAsync(ct);

        // Mirror to GitHub (fire-and-update; never blocks ticket creation)
        var labels   = BuildLabels(request.Category, request.Priority, TicketQueue.Client);
        var ghBody   = BuildIssueBody(request, id);
        var ghIssue  = await _github.CreateIssueAsync(request.Subject, ghBody, labels, ct);

        if (ghIssue is not null)
        {
            ticket.SetGitHubIssue(ghIssue.Number, ghIssue.HtmlUrl);
            await _uow.SaveChangesAsync(ct);
        }

        _logger.LogInformation(
            "EVT=TicketRaised CAT=Ticketing ACT=Raise OUT=Success TICKET={Id} ORG={Org} GITHUB={Issue}",
            id, request.OrgId, ghIssue?.Number.ToString() ?? "none");

        return Result.Success(id);
    }

    private static IReadOnlyList<string> BuildLabels(TicketCategory cat, TicketPriority pri, TicketQueue queue)
        => new[]
        {
            $"cat:{cat.ToString().ToLowerInvariant()}",
            $"priority:{pri.ToString().ToLowerInvariant()}",
            $"queue:{queue.ToString().ToLowerInvariant()}"
        };

    private static string BuildIssueBody(RaiseTicketCommand r, string ticketId)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"**Ticket ID:** `{ticketId}`");
        sb.AppendLine($"**Org:** `{r.OrgId}`");
        sb.AppendLine($"**User:** {r.RaisedByFirstName} (`{r.RaisedByUserId}`)");
        sb.AppendLine($"**Category:** {r.Category}");
        sb.AppendLine($"**Priority:** {r.Priority}");
        sb.AppendLine();
        sb.AppendLine("---");
        sb.AppendLine();
        sb.AppendLine(r.Description);

        if (!string.IsNullOrWhiteSpace(r.ErrorContext))
        {
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine("### System Error Context");
            sb.AppendLine("```json");
            sb.AppendLine(r.ErrorContext);
            sb.AppendLine("```");
        }

        return sb.ToString();
    }
}

// ── Add Comment / Reply ───────────────────────────────────────────────────────
public sealed record AddTicketCommentCommand(
    string TicketId,
    string AuthorUserId,
    string AuthorDisplayName,
    bool   CallerIsSupport,
    string Body,
    bool   IsPrivate) : IRequest<Result<string>>;

public sealed class AddTicketCommentHandler : IRequestHandler<AddTicketCommentCommand, Result<string>>
{
    private readonly ISupportTicketRepository _tickets;
    private readonly IGitHubTicketingService  _github;
    private readonly IUnitOfWork              _uow;
    private readonly ILogger<AddTicketCommentHandler> _logger;

    public AddTicketCommentHandler(
        ISupportTicketRepository tickets,
        IGitHubTicketingService github,
        IUnitOfWork uow,
        ILogger<AddTicketCommentHandler> logger)
    {
        _tickets = tickets;
        _github  = github;
        _uow     = uow;
        _logger  = logger;
    }

    public async Task<Result<string>> Handle(AddTicketCommentCommand request, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(request.TicketId, ct);
        if (ticket is null)   return Result.Failure<string>(TicketErrors.NotFound);
        if (ticket.Status == TicketStatus.Closed)
            return Result.Failure<string>(TicketErrors.AlreadyClosed);

        // Private notes can only be created by support staff
        if (request.IsPrivate && !request.CallerIsSupport)
            return Result.Failure<string>(TicketErrors.Forbidden);

        var id      = IdGenerator.New(IdPrefix.TicketComment);
        var comment = TicketComment.Create(
            id,
            request.TicketId,
            request.AuthorUserId,
            request.AuthorDisplayName,
            request.Body,
            request.IsPrivate);

        await _tickets.AddCommentAsync(comment, ct);

        // If ticket was awaiting user, move to in-progress on user reply
        if (!request.CallerIsSupport && ticket.Status == TicketStatus.AwaitingUser)
        {
            ticket.UpdateStatus(TicketStatus.InProgress);
        }

        await _uow.SaveChangesAsync(ct);

        // Mirror to GitHub
        if (ticket.GitHubIssueNumber.HasValue)
        {
            var prefix = request.IsPrivate ? "**[PRIVATE NOTE]** " : "";
            var ghBody = $"{prefix}**{request.AuthorDisplayName}:**\n\n{request.Body}";
            var ghComment = await _github.AddCommentAsync(ticket.GitHubIssueNumber.Value, ghBody, ct);
            if (ghComment is not null)
            {
                comment.SetGitHubCommentId(ghComment.Id);
                await _uow.SaveChangesAsync(ct);
            }
        }

        _logger.LogInformation(
            "EVT=TicketComment CAT=Ticketing ACT=AddComment OUT=Success TICKET={Ticket} PRIVATE={Private}",
            request.TicketId, request.IsPrivate);

        return Result.Success(id);
    }
}

// ── Escalate ──────────────────────────────────────────────────────────────────
public sealed record EscalateTicketCommand(
    string         TicketId,
    string         ActorUserId,
    string         ActorDisplayName,
    TicketQueue    ToQueue,
    TicketPriority? NewPriority,
    string?        Reason) : IRequest<Result>;

public sealed class EscalateTicketHandler : IRequestHandler<EscalateTicketCommand, Result>
{
    private readonly ISupportTicketRepository _tickets;
    private readonly IGitHubTicketingService  _github;
    private readonly IUnitOfWork              _uow;

    public EscalateTicketHandler(
        ISupportTicketRepository tickets,
        IGitHubTicketingService github,
        IUnitOfWork uow)
    {
        _tickets = tickets;
        _github  = github;
        _uow     = uow;
    }

    public async Task<Result> Handle(EscalateTicketCommand request, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(request.TicketId, ct);
        if (ticket is null) return Result.Failure(TicketErrors.NotFound);
        if (ticket.Status == TicketStatus.Closed) return Result.Failure(TicketErrors.AlreadyClosed);

        var escId = IdGenerator.New(IdPrefix.TicketEscalation);
        var esc   = TicketEscalation.Create(
            escId, request.TicketId, request.ActorUserId,
            ticket.Queue, request.ToQueue, isEscalation: true, request.Reason);

        ticket.Escalate(request.ToQueue, request.NewPriority);

        await _tickets.AddEscalationAsync(esc, ct);
        await _uow.SaveChangesAsync(ct);

        // GitHub: update labels + add system comment
        if (ticket.GitHubIssueNumber.HasValue)
        {
            var labels = new[] {
                $"cat:{ticket.Category.ToString().ToLowerInvariant()}",
                $"priority:{ticket.Priority.ToString().ToLowerInvariant()}",
                $"queue:{ticket.Queue.ToString().ToLowerInvariant()}",
                "escalated"
            };
            await _github.SetLabelsAsync(ticket.GitHubIssueNumber.Value, labels, ct);

            var note = $"**[ESCALATED]** Moved to queue `{request.ToQueue}` by {request.ActorDisplayName}."
                     + (string.IsNullOrWhiteSpace(request.Reason) ? "" : $"\n\nReason: {request.Reason}");
            await _github.AddCommentAsync(ticket.GitHubIssueNumber.Value, note, ct);
        }

        return Result.Success();
    }
}

// ── De-escalate ───────────────────────────────────────────────────────────────
public sealed record DeescalateTicketCommand(
    string      TicketId,
    string      ActorUserId,
    string      ActorDisplayName,
    TicketQueue ToQueue,
    string?     Reason) : IRequest<Result>;

public sealed class DeescalateTicketHandler : IRequestHandler<DeescalateTicketCommand, Result>
{
    private readonly ISupportTicketRepository _tickets;
    private readonly IGitHubTicketingService  _github;
    private readonly IUnitOfWork              _uow;

    public DeescalateTicketHandler(
        ISupportTicketRepository tickets,
        IGitHubTicketingService github,
        IUnitOfWork uow)
    {
        _tickets = tickets;
        _github  = github;
        _uow     = uow;
    }

    public async Task<Result> Handle(DeescalateTicketCommand request, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(request.TicketId, ct);
        if (ticket is null) return Result.Failure(TicketErrors.NotFound);

        var escId = IdGenerator.New(IdPrefix.TicketEscalation);
        var esc   = TicketEscalation.Create(
            escId, request.TicketId, request.ActorUserId,
            ticket.Queue, request.ToQueue, isEscalation: false, request.Reason);

        ticket.Deescalate(request.ToQueue);

        await _tickets.AddEscalationAsync(esc, ct);
        await _uow.SaveChangesAsync(ct);

        if (ticket.GitHubIssueNumber.HasValue)
        {
            var labels = new[] {
                $"cat:{ticket.Category.ToString().ToLowerInvariant()}",
                $"priority:{ticket.Priority.ToString().ToLowerInvariant()}",
                $"queue:{ticket.Queue.ToString().ToLowerInvariant()}"
            };
            await _github.SetLabelsAsync(ticket.GitHubIssueNumber.Value, labels, ct);

            var note = $"**[DE-ESCALATED]** Moved back to queue `{request.ToQueue}` by {request.ActorDisplayName}."
                     + (string.IsNullOrWhiteSpace(request.Reason) ? "" : $"\n\nReason: {request.Reason}");
            await _github.AddCommentAsync(ticket.GitHubIssueNumber.Value, note, ct);
        }

        return Result.Success();
    }
}

// ── Close Ticket ──────────────────────────────────────────────────────────────
public sealed record CloseTicketCommand(
    string  TicketId,
    string  ActorUserId,
    string  ActorDisplayName,
    string? ResolutionNote) : IRequest<Result>;

public sealed class CloseTicketHandler : IRequestHandler<CloseTicketCommand, Result>
{
    private readonly ISupportTicketRepository _tickets;
    private readonly IGitHubTicketingService  _github;
    private readonly IUnitOfWork              _uow;
    private readonly ILogger<CloseTicketHandler> _logger;

    public CloseTicketHandler(
        ISupportTicketRepository tickets,
        IGitHubTicketingService github,
        IUnitOfWork uow,
        ILogger<CloseTicketHandler> logger)
    {
        _tickets = tickets;
        _github  = github;
        _uow     = uow;
        _logger  = logger;
    }

    public async Task<Result> Handle(CloseTicketCommand request, CancellationToken ct)
    {
        var ticket = await _tickets.GetByIdAsync(request.TicketId, ct);
        if (ticket is null) return Result.Failure(TicketErrors.NotFound);
        if (ticket.Status == TicketStatus.Closed) return Result.Failure(TicketErrors.AlreadyClosed);

        ticket.UpdateStatus(TicketStatus.Closed);
        await _uow.SaveChangesAsync(ct);

        if (ticket.GitHubIssueNumber.HasValue)
        {
            if (!string.IsNullOrWhiteSpace(request.ResolutionNote))
            {
                var note = $"**[RESOLVED]** {request.ActorDisplayName}: {request.ResolutionNote}";
                await _github.AddCommentAsync(ticket.GitHubIssueNumber.Value, note, ct);
            }
            await _github.CloseIssueAsync(ticket.GitHubIssueNumber.Value, ct);
        }

        _logger.LogInformation(
            "EVT=TicketClosed CAT=Ticketing ACT=Close OUT=Success TICKET={Id}",
            request.TicketId);

        return Result.Success();
    }
}

// ── Upload Attachment ──────────────────────────────────────────────────────────
public sealed record UploadAttachmentCommand(
    string TicketId,
    string UploaderUserId,
    bool   UploaderIsSupport,
    string FileName,
    string ContentType,
    byte[] Content,
    bool   IsPrivate) : IRequest<Result<string>>;

public static class AttachmentErrors
{
    public static readonly Error FileTooLarge = new("attachment.too_large",
        "Attachment exceeds the maximum allowed size of 10 MB.");
    public static readonly Error InvalidType  = new("attachment.invalid_type",
        "The file type is not permitted.");
}

public sealed class UploadAttachmentHandler : IRequestHandler<UploadAttachmentCommand, Result<string>>
{
    private const long MaxBytes = 10 * 1024 * 1024; // 10 MB

    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/png", "image/jpeg", "image/gif", "image/webp",
        "application/pdf",
        "text/plain",
        "application/zip",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",   // .docx
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",         // .xlsx
        "application/vnd.ms-excel",
        "video/mp4"
    };

    private readonly ISupportTicketRepository          _tickets;
    private readonly IUnitOfWork                       _uow;
    private readonly ILogger<UploadAttachmentHandler>  _logger;

    public UploadAttachmentHandler(
        ISupportTicketRepository tickets,
        IUnitOfWork uow,
        ILogger<UploadAttachmentHandler> logger)
    {
        _tickets = tickets;
        _uow     = uow;
        _logger  = logger;
    }

    public async Task<Result<string>> Handle(UploadAttachmentCommand request, CancellationToken ct)
    {
        if (request.Content.Length > MaxBytes)
            return Result.Failure<string>(AttachmentErrors.FileTooLarge);

        if (!AllowedTypes.Contains(request.ContentType))
            return Result.Failure<string>(AttachmentErrors.InvalidType);

        var ticket = await _tickets.GetByIdAsync(request.TicketId, ct);
        if (ticket is null)  return Result.Failure<string>(TicketErrors.NotFound);
        if (ticket.Status == TicketStatus.Closed)
            return Result.Failure<string>(TicketErrors.AlreadyClosed);

        // Private attachments only allowed from support staff
        if (request.IsPrivate && !request.UploaderIsSupport)
            return Result.Failure<string>(TicketErrors.Forbidden);

        var id = IdGenerator.New(IdPrefix.TicketAttachment);
        var attachment = TicketAttachment.Create(
            id,
            request.TicketId,
            request.UploaderUserId,
            request.FileName,
            request.ContentType,
            request.Content,
            request.IsPrivate);

        await _tickets.AddAttachmentAsync(attachment, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation(
            "EVT=AttachmentUploaded CAT=Ticketing ACT=Upload OUT=Success TICKET={Ticket} ATTACH={Id} SIZE={Size}",
            request.TicketId, id, request.Content.Length);

        return Result.Success(id);
    }
}
