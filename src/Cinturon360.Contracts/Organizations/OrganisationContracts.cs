namespace Cinturon360.Contracts.Organizations;

public sealed record CreateOrganisationRequest(
    string Name,
    string Slug,
    string OrgType,
    string? ParentOrgId = null,
    string? PrimaryEmail = null,
    string LanguageCode = "en",
    string TimeZone = "UTC",
    string CurrencyCode = "USD"
);

public sealed record UpdateOrganisationRequest(
    string Name,
    string? PrimaryEmail,
    string? PrimaryPhone,
    string? Website,
    string? SupportTicketEmailTemplateCode,
    string LanguageCode,
    string TimeZone,
    string CurrencyCode
);

public sealed record OrganisationSummary(
    string OrgId,
    string Name,
    string Slug,
    string OrgType,
    string? ParentOrgId,
    bool IsActive,
    DateTimeOffset CreatedAt
);

public sealed record OrganisationDetail(
    string OrgId,
    string Name,
    string Slug,
    string OrgType,
    string? ParentOrgId,
    bool IsActive,
    string? PrimaryEmail,
    string? PrimaryPhone,
    string? Website,
    string? SupportTicketEmailTemplateCode,
    string LanguageCode,
    string TimeZone,
    string CurrencyCode,
    DateTimeOffset CreatedAt
);

public sealed record OrganisationHierarchyNode(
    string OrgId,
    string Name,
    string OrgType,
    bool IsActive,
    IReadOnlyList<OrganisationHierarchyNode> Children
);
