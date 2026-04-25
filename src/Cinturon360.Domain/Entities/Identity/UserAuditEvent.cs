using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// Append-only audit log entry per user action. Never updated or deleted.
/// </summary>
public sealed class UserAuditEvent : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public AuditEventType EventType { get; private set; }
    public string? ActorUserId { get; private set; }
    public string? OrgId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }

    /// <summary>Free-form JSON details for the specific event.</summary>
    public string? Details { get; private set; }

    public bool Success { get; private set; }
    public string? FailureReason { get; private set; }

    private UserAuditEvent() { }

    public static UserAuditEvent Record(
        string id,
        string userId,
        AuditEventType eventType,
        bool success,
        string? actorUserId = null,
        string? orgId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        string? details = null,
        string? failureReason = null)
    {
        return new UserAuditEvent
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            UserId = userId,
            EventType = eventType,
            Success = success,
            ActorUserId = actorUserId,
            OrgId = orgId,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CorrelationId = correlationId,
            Details = details,
            FailureReason = failureReason
        };
    }
}
