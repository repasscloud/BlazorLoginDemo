using Cinturon360.Domain.Entities.Ticketing;

namespace Cinturon360.Application.Abstractions.Services;

public interface ITicketUpdateNotificationService
{
    Task NotifyPublicUpdateAsync(
        SupportTicket ticket,
        string updateType,
        string actorUserId,
        string actorDisplayName,
        string body,
        CancellationToken ct = default);
}