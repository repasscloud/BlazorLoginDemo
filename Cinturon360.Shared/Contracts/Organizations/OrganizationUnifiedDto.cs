namespace Cinturon360.Shared.Contracts.Organizations;

// ===============================
// ORGANIZATION (UNIFIED) DTO
// ===============================
public class OrganizationUnifiedDto
{
    // convenience aggregates
    // public sealed record OrganizationAggregate(
    //     IReadOnlyList<Models.Organizations.Organization> Organizations,
    //     CommandMetadata Metadata
    // );

    public sealed record CreateOrganizationAggregate(
        CreateOrganizationRequest CreateOrganization,
        CommandMetadata Metadata
    );

    // public sealed record UpdateOrganizationAggregate(
    //     UpdateOrganizationRequest UpdateOrganization,
    //     CommandMetadata Metadata
    // );

    public sealed record OrganizationNoResponseAggregate(
        string OrganizationId,
        CommandMetadata Metadata
    );

    // public sealed record ListOrganizationItemsAggregate(
    //     IReadOnlyList<Models.Organizations.OrganizationListItem> OrganizationItems,
    //     CommandMetadata Metadata
    // );
}
