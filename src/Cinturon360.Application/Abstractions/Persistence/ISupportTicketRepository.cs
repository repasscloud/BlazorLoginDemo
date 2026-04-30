using Cinturon360.Domain.Entities.Ticketing;
using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Application.Abstractions.Persistence;

/// <summary>
/// Repository for support tickets, comments, and escalation history.
/// </summary>
public interface ISupportTicketRepository
{
    Task<SupportTicket?> GetByIdAsync(string ticketId, CancellationToken ct = default);

    /// <summary>
    /// Full detail including comments and escalations.
    /// Comments filtered: private notes are only returned when callerIsSupport = true.
    /// </summary>
    Task<SupportTicket?> GetDetailAsync(string ticketId, bool callerIsSupport, CancellationToken ct = default);

    /// <summary>
    /// Tickets raised by a specific user (all orgs). Public-facing: no private notes loaded.
    /// </summary>
    Task<(IReadOnlyList<SupportTicket> Items, int Total)> ListByUserAsync(
        string userId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// All tickets for an org (support dashboard). Includes private comment counts.
    /// </summary>
    Task<(IReadOnlyList<SupportTicket> Items, int Total)> ListByOrgAsync(
        string orgId, TicketStatus? status, TicketQueue? queue, int page, int pageSize,
        CancellationToken ct = default);

    Task AddAsync(SupportTicket ticket, CancellationToken ct = default);
    Task AddCommentAsync(TicketComment comment, CancellationToken ct = default);
    Task AddEscalationAsync(TicketEscalation escalation, CancellationToken ct = default);
    Task AddAttachmentAsync(TicketAttachment attachment, CancellationToken ct = default);

    /// <summary>
    /// Returns a single attachment by ID including its binary content.
    /// Used for the download endpoint.
    /// </summary>
    Task<TicketAttachment?> GetAttachmentAsync(string attachmentId, CancellationToken ct = default);
}
