using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>Emergency contact for a user traveller.</summary>
public sealed class UserEmergencyContact : Entity
{
    public string UserId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Relationship { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string? Email { get; private set; }
    public bool IsPrimary { get; private set; }

    private UserEmergencyContact() { }

    public static UserEmergencyContact Create(
        string id,
        string userId,
        string name,
        string relationship,
        string phone,
        string? email = null,
        bool isPrimary = false)
        => new()
        {
            Id = id,
            UserId = userId,
            Name = name,
            Relationship = relationship,
            Phone = phone,
            Email = email,
            IsPrimary = isPrimary,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Update(string name, string relationship, string phone, string? email)
    {
        Name = name;
        Relationship = relationship;
        Phone = phone;
        Email = email;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPrimary(bool isPrimary)
    {
        IsPrimary = isPrimary;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
