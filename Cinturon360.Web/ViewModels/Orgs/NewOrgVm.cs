using Cinturon360.Web.ViewModels.ReferenceData;

namespace Cinturon360.Web.ViewModels.Orgs;

public sealed record NewOrgVm
{
    // Context
    public string Rid { get; init; } = default!;
    public string OrgId { get; init; } = default!;
    public string UserId { get; init; } = default!;
    public string TravelPolicyId { get; init; } = default!;

    // Reference data (UI only)
    public OrgTypeReference OrgTypeRef { get; init; } = default!;
    public OrgSectorReference OrgSectorRef { get; init; } = default!;
    public OrgClassReference OrgClassRef { get; init; } = default!;
    public SlaTierReference SlaTierRef { get; init; } = default!;

    public CurrencyReference CurrencyRef { get; init; } = default!;
    public IDDReference IDDRef { get; init; } = default!;

    // tax id type (comes from TaxationType)
    
    public TimeZoneReference TimeZoneRef { get; init; } = default!;
    public GeographyReference GeographyRef { get; init; } = default!;  // we use the country list in this for org's location only


    // UI state
    public bool IsLoading { get; set; }
    public string? Error { get; set; }
}
