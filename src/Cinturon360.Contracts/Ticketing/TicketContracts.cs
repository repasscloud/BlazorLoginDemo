using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Contracts.Ticketing;

// ── Requests ───────────────────────────────────────────────────────────────

/// <summary>
/// Raise a new support ticket.
/// ErrorContext is a JSON string populated when the user raises from an error page.
/// </summary>
public sealed record RaiseTicketRequest(
    TicketCategory Category,
    TicketPriority Priority,
    string Subject,
    string Description,
    string? ErrorContext);   // JSON: page, errorCode, traceId, message, timestamp

public sealed record ReplyToTicketRequest(
    string Body,
    bool   IsPrivate);   // private notes only visible to support staff

public sealed record EscalateTicketRequest(
    TicketQueue ToQueue,
    TicketPriority? NewPriority,
    string? Reason);

public sealed record DeescalateTicketRequest(
    TicketQueue ToQueue,
    string?     Reason);

public sealed record CloseTicketRequest(
    string? ResolutionNote);

// ── Responses ──────────────────────────────────────────────────────────────

public sealed record TicketSummaryResponse(
    string         Id,
    string         RaisedByUserId,
    string         Subject,
    TicketStatus   Status,
    TicketPriority Priority,
    TicketQueue    Queue,
    TicketCategory Category,
    int?           GitHubIssueNumber,
    string?        GitHubIssueUrl,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record TicketCommentResponse(
    string         Id,
    string         AuthorDisplayName,
    string         Body,
    bool           IsPrivate,
    DateTimeOffset CreatedAt);

public sealed record TicketEscalationResponse(
    string         Id,
    TicketQueue    FromQueue,
    TicketQueue    ToQueue,
    bool           IsEscalation,
    string?        Reason,
    DateTimeOffset CreatedAt);

public sealed record TicketAttachmentResponse(
    string         Id,
    string         FileName,
    string         ContentType,
    long           FileSize,
    bool           IsPrivate,
    DateTimeOffset CreatedAt);

public sealed record TicketDetailResponse(
    TicketSummaryResponse                    Ticket,
    IReadOnlyList<TicketCommentResponse>    Comments,
    IReadOnlyList<TicketEscalationResponse> Escalations,
    IReadOnlyList<TicketAttachmentResponse> Attachments);

public sealed record TicketListResponse(
    IReadOnlyList<TicketSummaryResponse> Items,
    int Total);
