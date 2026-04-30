using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Ticketing;

namespace Cinturon360.Domain.Entities.Ticketing;

/// <summary>
/// Records a single escalation or de-escalation event on a ticket.
/// </summary>
public sealed class TicketEscalation : Entity
{
    public string      TicketId       { get; private set; } = string.Empty;
    public string      ActorUserId    { get; private set; } = string.Empty;
    public TicketQueue FromQueue      { get; private set; }
    public TicketQueue ToQueue        { get; private set; }
    public bool        IsEscalation   { get; private set; }  // false = de-escalation
    public string?     Reason         { get; private set; }

    private TicketEscalation() { }

    public static TicketEscalation Create(
        string id,
        string ticketId,
        string actorUserId,
        TicketQueue fromQueue,
        TicketQueue toQueue,
        bool isEscalation,
        string? reason = null)
    {
        return new TicketEscalation
        {
            Id           = id,
            CreatedAt    = DateTimeOffset.UtcNow,
            UpdatedAt    = DateTimeOffset.UtcNow,
            TicketId     = ticketId,
            ActorUserId  = actorUserId,
            FromQueue    = fromQueue,
            ToQueue      = toQueue,
            IsEscalation = isEscalation,
            Reason       = reason
        };
    }
}
