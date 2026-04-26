namespace Cinturon360.Contracts.Organizations;

/// <summary>Alias for single organisation detail response.</summary>
public sealed record OrganisationResponse(
    string OrgId,
    string Name,
    string Slug,
    string OrgType,
    string? ParentOrgId,
    bool IsActive,
    string? PrimaryEmail,
    string? PrimaryPhone,
    string? Website,
    string LanguageCode,
    string TimeZone,
    string CurrencyCode,
    DateTimeOffset CreatedAt);

/// <summary>List wrapper for organisations.</summary>
public sealed record OrganisationListResponse(
    IReadOnlyList<OrganisationSummary> Items,
    int Total,
    int Page,
    int PageSize);
