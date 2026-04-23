namespace Cinturon360.Domain.Common.Interfaces;

/// <summary>
/// Marker interface for domain events.
/// Handlers live in Application layer (MediatR INotificationHandler).
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
