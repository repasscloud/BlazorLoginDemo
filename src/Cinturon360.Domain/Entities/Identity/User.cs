using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Domain.Entities.Identity;

/// <summary>
/// A platform principal. Covers all human users across all org types plus platform accounts.
/// Auth credential data lives in UserSecurity / UserAuthMethod — not here.
/// </summary>
public sealed class User : SoftDeletableEntity
{
    // ── Classification ───────────────────────────────────────────────────
    public UserCategory UserCategory { get; private set; }

    /// <summary>Populated only for Platform-category users. Null for all tenant users.</summary>
    public PlatformRole? PlatformRole { get; private set; }

    /// <summary>The org this user belongs to. Null for Platform users who have no home org.</summary>
    public string? HomeOrgId { get; private set; }

    // ── Identity ─────────────────────────────────────────────────────────
    public string Email { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailVerified { get; private set; }

    // ── Account state ─────────────────────────────────────────────────────
    public bool IsActive { get; private set; } = true;
    public bool IsLocked { get; private set; }
    public bool IsSuspended { get; private set; }

    // ── Locale ───────────────────────────────────────────────────────────
    public string LanguageCode { get; private set; } = "en";
    public string TimeZone { get; private set; } = "UTC";
    public string CurrencyCode { get; private set; } = "USD";

    // ── Profile ──────────────────────────────────────────────────────────
    public string? AvatarStorageKey { get; private set; }
    public DateTimeOffset? LastLoginAt { get; private set; }

    // ── Concurrency ──────────────────────────────────────────────────────
    public string ConcurrencyStamp { get; private set; } = Guid.NewGuid().ToString();

    private User() { }

    public static User Create(
        string id,
        string email,
        string firstName,
        string lastName,
        UserCategory category,
        string? homeOrgId = null,
        PlatformRole? platformRole = null)
    {
        return new User
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Email = email.ToLowerInvariant().Trim(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            UserCategory = category,
            HomeOrgId = homeOrgId,
            PlatformRole = platformRole,
            IsActive = true,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public void RecordLogin() => LastLoginAt = DateTimeOffset.UtcNow;

    public void Lock()       { IsLocked    = true;  UpdatedAt = DateTimeOffset.UtcNow; }
    public void Unlock()     { IsLocked    = false; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Suspend()    { IsSuspended = true;  UpdatedAt = DateTimeOffset.UtcNow; }
    public void Reactivate() { IsSuspended = false; IsActive = true; UpdatedAt = DateTimeOffset.UtcNow; }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLocale(string languageCode, string timeZone, string currencyCode)
    {
        LanguageCode = languageCode;
        TimeZone = timeZone;
        CurrencyCode = currencyCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
