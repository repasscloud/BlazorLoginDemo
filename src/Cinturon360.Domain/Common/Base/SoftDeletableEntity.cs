namespace Cinturon360.Domain.Common.Base;

/// <summary>
/// Marks a domain entity that supports soft-deletion.
/// </summary>
public abstract class SoftDeletableEntity : Entity
{
    public bool IsDeleted { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
    public string? DeletedByUserId { get; protected set; }

    protected SoftDeletableEntity() { }

    protected SoftDeletableEntity(string id) : base(id) { }

    public virtual void SoftDelete(string deletedByUserId)
    {
        IsDeleted = true;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedByUserId = deletedByUserId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
