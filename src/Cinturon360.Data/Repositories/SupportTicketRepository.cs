using Microsoft.EntityFrameworkCore;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Data.Context;
using Cinturon360.Domain.Entities.Ticketing;
using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Data.Repositories;

public sealed class SupportTicketRepository(AppDbContext db) : ISupportTicketRepository
{
    public Task<SupportTicket?> GetByIdAsync(string ticketId, CancellationToken ct = default)
        => db.SupportTickets
            .FirstOrDefaultAsync(t => t.Id == ticketId, ct);

    public Task<SupportTicket?> GetDetailAsync(string ticketId, bool callerIsSupport, CancellationToken ct = default)
    {
        var query = db.SupportTickets
            .Include(t => t.Escalations)
            .Where(t => t.Id == ticketId);

        if (callerIsSupport)
        {
            query = query
                .Include(t => t.Comments)
                .Include(t => t.Attachments);
        }
        else
        {
            query = query
                .Include(t => t.Comments.Where(c => !c.IsPrivate))
                .Include(t => t.Attachments.Where(a => !a.IsPrivate));
        }

        return query.FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<SupportTicket> Items, int Total)> ListByUserAsync(
        string userId, int page, int pageSize, CancellationToken ct = default)
    {
        var q = db.SupportTickets
            .Where(t => t.RaisedByUserId == userId)
            .OrderByDescending(t => t.UpdatedAt);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IReadOnlyList<SupportTicket> Items, int Total)> ListByOrgAsync(
        string orgId, TicketStatus? status, TicketQueue? queue, int page, int pageSize,
        CancellationToken ct = default)
    {
        var q = db.SupportTickets.Where(t => t.OrgId == orgId);

        if (status.HasValue) q = q.Where(t => t.Status == status.Value);
        if (queue.HasValue)  q = q.Where(t => t.Queue  == queue.Value);

        q = q.OrderByDescending(t => t.UpdatedAt);

        var total = await q.CountAsync(ct);
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(SupportTicket ticket, CancellationToken ct = default)
        => await db.SupportTickets.AddAsync(ticket, ct);

    public async Task AddCommentAsync(TicketComment comment, CancellationToken ct = default)
        => await db.TicketComments.AddAsync(comment, ct);

    public async Task AddEscalationAsync(TicketEscalation escalation, CancellationToken ct = default)
        => await db.TicketEscalations.AddAsync(escalation, ct);

    public async Task AddAttachmentAsync(TicketAttachment attachment, CancellationToken ct = default)
        => await db.TicketAttachments.AddAsync(attachment, ct);

    public Task<TicketAttachment?> GetAttachmentAsync(string attachmentId, CancellationToken ct = default)
        => db.TicketAttachments.FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
}
