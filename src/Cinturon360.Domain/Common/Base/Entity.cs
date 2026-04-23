using Cinturon360.Domain.Common.Interfaces;

namespace Cinturon360.Domain.Common.Base;

/// <summary>
/// Base class for all domain entities. IDs are string (prefixed NanoIds) assigned at creation.
/// </summary>
public abstract class Entity
{
    public string Id { get; protected init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; protected init; }
    public DateTimeOffset UpdatedAt { get; protected set; }

    protected Entity() { }

    protected Entity(string id)
    {
        Id = id;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent @event) => _domainEvents.Add(@event);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
