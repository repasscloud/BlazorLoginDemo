using Cinturon360.Domain.Common.Base;
using Cinturon360.Domain.Enums.System;

namespace Cinturon360.Domain.Entities.Organization;

/// <summary>
/// A tenant organisation. Can be a Vendor, TMC, or Client type.
/// All data, roles, and billing is scoped by OrgId.
/// Hierarchy: Vendor → TMC → Client. Access never flows upward or sideways.
/// </summary>
public sealed class Organisation : SoftDeletableEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public OrgType OrgType { get; private set; }
    public string? ParentOrgId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? LogoStorageKey { get; private set; }

    // Contact info
    public string? PrimaryEmail { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public string? Website { get; private set; }

    // Locale
    public string LanguageCode { get; private set; } = "en";
    public string TimeZone { get; private set; } = "UTC";
    public string CurrencyCode { get; private set; } = "USD";

    // Metadata
    public string? ExternalRef { get; private set; }

    private Organisation() { }

    public static Organisation Create(
        string id,
        string name,
        string slug,
        OrgType orgType,
        string? parentOrgId = null,
        string? primaryEmail = null,
        string languageCode = "en",
        string timeZone = "UTC",
        string currencyCode = "USD")
    {
        return new Organisation
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Name = name.Trim(),
            Slug = slug.ToLowerInvariant().Trim(),
            OrgType = orgType,
            ParentOrgId = parentOrgId,
            IsActive = true,
            PrimaryEmail = primaryEmail,
            LanguageCode = languageCode,
            TimeZone = timeZone,
            CurrencyCode = currencyCode
        };
    }

    public void UpdateDetails(string name, string? primaryEmail, string? primaryPhone, string? website)
    {
        Name = name.Trim();
        PrimaryEmail = primaryEmail;
        PrimaryPhone = primaryPhone;
        Website = website;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLocale(string languageCode, string timeZone, string currencyCode)
    {
        LanguageCode = languageCode;
        TimeZone = timeZone;
        CurrencyCode = currencyCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetLogo(string storageKey)
    {
        LogoStorageKey = storageKey;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetExternalRef(string externalRef)
    {
        ExternalRef = externalRef;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Reactivate()
    {
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
