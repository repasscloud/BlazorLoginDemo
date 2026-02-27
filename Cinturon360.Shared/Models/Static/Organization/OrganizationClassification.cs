namespace Cinturon360.Shared.Models.Static.Organization;

/// <summary>
/// Static reference data for organization classifications.
/// Record model is the source of truth.
/// Enum exists for legacy compatibility only.
/// </summary>
public static partial class OrganizationClassifications
{
    // ------------------------------------------------------------------------
    // LEGACY ENUM (do NOT remove — used by existing aggregates, DB mappings)
    // ------------------------------------------------------------------------

    public enum OrganizationClassification : int
    {
        Unknown = 0,

        // Size progression (auto-upgradable)
        PreStartup = 10,
        Startup = 20,
        ScaleUp = 30,

        MicroBusiness = 40,
        SmallBusiness = 50,
        LowerMidMarket = 60,
        UpperMidMarket = 70,
        Corporate = 80,
        Enterprise = 90,
        GlobalEnterprise = 100,

        // Special identity (manual override)
        GovernmentAgency = 200,
        StateOwnedEnterprise = 210,
        PublicListedCompany = 220,
        PrivateEquityBacked = 230,
        FamilyOwnedBusiness = 240,

        NonProfit = 300,
        Charity = 310,
        Foundation = 320,
        Association = 330
    }

    // ------------------------------------------------------------------------
    // NEW RECORD MODEL (source of truth)
    // ------------------------------------------------------------------------

    public sealed record OrganizationClassificationInfo(
        int Id,
        string Code,
        string CodeLower,
        string Name,
        string Description,
        bool AutoAssignable
    )
    {
        public OrganizationClassification Enum =>
            (OrganizationClassification)Id;
    }

    // ------------------------------------------------------------------------
    // SINGLE SOURCE OF TRUTH
    // ------------------------------------------------------------------------

    private static readonly IReadOnlyList<OrganizationClassificationInfo> _all =
    [
        new(0,   "UNKNOWN", "unknown", "Unknown", "Unspecified classification", false),

        // Auto progression
        new(10,  "PRE_STARTUP", "pre_startup", "Pre-Startup", "1–5 licensed users", true),
        new(20,  "STARTUP", "startup", "Startup", "6–25 licensed users", true),
        new(30,  "SCALE_UP", "scale_up", "Scale-Up", "26–75 licensed users", true),

        new(40,  "MICRO_BUSINESS", "micro_business", "Micro Business", "1–10 licensed users", true),
        new(50,  "SMALL_BUSINESS", "small_business", "Small Business", "11–50 licensed users", true),
        new(60,  "LOWER_MID_MARKET", "lower_mid_market", "Lower Mid-Market", "51–250 licensed users", true),
        new(70,  "UPPER_MID_MARKET", "upper_mid_market", "Upper Mid-Market", "251–1000 licensed users", true),
        new(80,  "CORPORATE", "corporate", "Corporate", "1001–5000 licensed users", true),
        new(90,  "ENTERPRISE", "enterprise", "Enterprise", "5001–20000 licensed users", true),
        new(100, "GLOBAL_ENTERPRISE", "global_enterprise", "Global Enterprise", "20000+ licensed users", true),

        // Manual only
        new(200, "GOVERNMENT_AGENCY", "government_agency", "Government Agency", "Government organization", false),
        new(210, "STATE_OWNED_ENTERPRISE", "state_owned_enterprise", "State-Owned Enterprise", "Government-owned corporation", false),
        new(220, "PUBLIC_LISTED_COMPANY", "public_listed_company", "Public Listed Company", "Publicly traded company", false),
        new(230, "PRIVATE_EQUITY_BACKED", "private_equity_backed", "Private Equity Backed", "Private equity owned company", false),
        new(240, "FAMILY_OWNED_BUSINESS", "family_owned_business", "Family-Owned Business", "Privately family-owned business", false),

        new(300, "NON_PROFIT", "non_profit", "Non-Profit", "Non-profit organization", false),
        new(310, "CHARITY", "charity", "Charity", "Registered charity", false),
        new(320, "FOUNDATION", "foundation", "Foundation", "Foundation organization", false),
        new(330, "ASSOCIATION", "association", "Association", "Professional or trade association", false)
    ];

    public static IReadOnlyList<OrganizationClassificationInfo> All => _all;

    // ------------------------------------------------------------------------
    // LOOKUPS
    // ------------------------------------------------------------------------

    public static OrganizationClassificationInfo Get(int id)
        => _all.First(x => x.Id == id);

    public static OrganizationClassificationInfo Get(OrganizationClassification type)
        => Get((int)type);

    public static OrganizationClassificationInfo? TryGet(int id)
        => _all.FirstOrDefault(x => x.Id == id);

    public static OrganizationClassification ToEnum(int id)
        => (OrganizationClassification)id;

    public static int ToId(OrganizationClassification type)
        => (int)type;

    // ------------------------------------------------------------------------
    // STRONGLY TYPED ACCESSORS
    // ------------------------------------------------------------------------

    public static OrganizationClassificationInfo Unknown => Get(0);
    public static OrganizationClassificationInfo Startup => Get(20);
    public static OrganizationClassificationInfo Enterprise => Get(90);
    public static OrganizationClassificationInfo GlobalEnterprise => Get(100);

    // ------------------------------------------------------------------------
    // UI PROJECTION
    // ------------------------------------------------------------------------

    public static IReadOnlyList<(OrganizationClassification Type, string Name)> DropdownOptions =>
        _all
            .Select(x => ((OrganizationClassification)x.Id, x.Name))
            .ToList();

    public static IReadOnlyList<(OrganizationClassification Type, string Name)> AutoAssignableOptions =>
        _all
            .Where(x => x.AutoAssignable)
            .Select(x => ((OrganizationClassification)x.Id, x.Name))
            .ToList();

    public static IReadOnlyList<(OrganizationClassification Type, string Name)> ManualOnlyOptions =>
        _all
            .Where(x => !x.AutoAssignable && x.Id != 0)
            .Select(x => ((OrganizationClassification)x.Id, x.Name))
            .ToList();
}