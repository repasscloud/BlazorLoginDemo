using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// A platform user. Identity/auth data lives in OpenIddict tables.
/// This entity holds profile and org membership data.
/// </summary>
public sealed class User : SoftDeletableEntity
{
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailVerified { get; private set; }
    public bool IsMfaEnabled { get; private set; }
    public string? AvatarStorageKey { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    // Navigation: org memberships resolved via OrgMembership entity
    private User() { }

    public static User Create(string id, string email, string firstName, string lastName)
    {
        return new User
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Email = email.ToLowerInvariant().Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim()
        };
    }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
