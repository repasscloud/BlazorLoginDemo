using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Domain.Entities.Ticketing;

/// <summary>
/// A support ticket raised by a user. Mirrored to a GitHub Issue when created.
/// All user-visible state lives here; GitHub is the external audit trail.
/// </summary>
public sealed class SupportTicket : Entity
{
    // ── Ownership ──────────────────────────────────────────────────────────
    /// <summary>The org the requester belongs to.</summary>
    public string OrgId { get; private set; } = string.Empty;

    /// <summary>User ID of the person who raised the ticket.</summary>
    public string RaisedByUserId { get; private set; } = string.Empty;

    // ── Classification ─────────────────────────────────────────────────────
    public TicketStatus   Status   { get; private set; }
    public TicketPriority Priority { get; private set; }
    public TicketQueue    Queue    { get; private set; }
    public TicketCategory Category { get; private set; }

    // ── Content ────────────────────────────────────────────────────────────
    public string Subject     { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Structured context attached when raised from an error page.
    /// JSON payload: { "page": "/client/bookings/bkg_xxx", "errorCode": "booking.not_found",
    ///                 "traceId": "abc123", "message": "...", "timestamp": "..." }
    /// Null when raised manually.
    /// </summary>
    public string? ErrorContext { get; private set; }

    /// <summary>
    /// Whether the ticket owner wants public updates emailed to them.
    /// Defaults to false and can be changed per ticket.
    /// </summary>
    public bool EmailMeUpdates { get; private set; }

    // ── GitHub mirror ──────────────────────────────────────────────────────
    /// <summary>GitHub issue number in the ticketing repo. Null until sync completes.</summary>
    public int? GitHubIssueNumber { get; private set; }

    /// <summary>Full GitHub issue URL, for deep-linking from the dashboard.</summary>
    public string? GitHubIssueUrl { get; private set; }

    // ── Resolution ─────────────────────────────────────────────────────────
    public DateTimeOffset? ResolvedAt { get; private set; }
    public DateTimeOffset? ClosedAt   { get; private set; }

    // ── Navigation ─────────────────────────────────────────────────────────
    private readonly List<TicketComment>     _comments     = [];
    private readonly List<TicketEscalation>  _escalations  = [];
    private readonly List<TicketAttachment>  _attachments  = [];

    public IReadOnlyList<TicketComment>     Comments     => _comments.AsReadOnly();
    public IReadOnlyList<TicketEscalation>  Escalations  => _escalations.AsReadOnly();
    public IReadOnlyList<TicketAttachment>  Attachments  => _attachments.AsReadOnly();

    private SupportTicket() { }

    public static SupportTicket Create(
        string id,
        string orgId,
        string raisedByUserId,
        TicketCategory category,
        TicketPriority priority,
        string subject,
        string description,
        bool emailMeUpdates = false,
        string? errorContext = null)
    {
        return new SupportTicket
        {
            Id              = id,
            CreatedAt       = DateTimeOffset.UtcNow,
            UpdatedAt       = DateTimeOffset.UtcNow,
            OrgId           = orgId,
            RaisedByUserId  = raisedByUserId,
            Category        = category,
            Priority        = priority,
            Status          = TicketStatus.Open,
            Queue           = TicketQueue.Client,
            Subject         = subject.Trim(),
            Description     = description.Trim(),
            EmailMeUpdates  = emailMeUpdates,
            ErrorContext    = errorContext
        };
    }

    public void SetEmailMeUpdates(bool enabled)
    {
        EmailMeUpdates = enabled;
        UpdatedAt      = DateTimeOffset.UtcNow;
    }

    public void SetGitHubIssue(int issueNumber, string issueUrl)
    {
        GitHubIssueNumber = issueNumber;
        GitHubIssueUrl    = issueUrl;
        UpdatedAt         = DateTimeOffset.UtcNow;
    }

    public void UpdateStatus(TicketStatus newStatus)
    {
        Status    = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;

        if (newStatus == TicketStatus.Resolved && ResolvedAt is null)
            ResolvedAt = DateTimeOffset.UtcNow;

        if (newStatus == TicketStatus.Closed && ClosedAt is null)
            ClosedAt = DateTimeOffset.UtcNow;
    }

    public void Escalate(TicketQueue toQueue, TicketPriority? newPriority = null)
    {
        Queue     = toQueue;
        Status    = TicketStatus.Escalated;
        Priority  = newPriority ?? Priority;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deescalate(TicketQueue toQueue)
    {
        Queue     = toQueue;
        Status    = TicketStatus.InProgress;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
