namespace Cinturon360.Shared.Models.Static.Organization;

/// <summary>
/// Static reference data for organization sectors.
/// Record model is the source of truth.
/// Enum exists for legacy compatibility only.
/// </summary>
public static partial class OrganizationSectors
{
    // ------------------------------------------------------------------------
    // LEGACY ENUM (do NOT remove — used by existing aggregates, DB mappings)
    // ------------------------------------------------------------------------

    public enum OrganizationSector : int
    {
        Unknown = 0,

        // Public / Civic
        Government = 10,
        Defence = 20,
        PublicAdministration = 30,

        // Education
        PrimaryEducation = 40,
        SecondaryEducation = 50,
        HigherEducation = 60,
        ResearchInstitution = 70,

        // Healthcare
        Healthcare = 80,
        Hospital = 90,
        Pharmaceutical = 100,
        Biotechnology = 110,

        // Financial
        Banking = 120,
        Insurance = 130,
        InvestmentManagement = 140,
        FinTech = 150,

        // Technology
        Technology = 160,
        Software = 170,
        SaaS = 180,
        Telecommunications = 190,
        CyberSecurity = 200,

        // Commercial industries
        Manufacturing = 210,
        Retail = 220,
        ECommerce = 230,
        Logistics = 240,
        Transportation = 250,
        Energy = 260,
        Mining = 270,
        Construction = 280,
        RealEstate = 290,
        Hospitality = 300,
        TravelAndTourism = 310,
        MediaAndEntertainment = 320,
        Agriculture = 330,

        // Services
        Consulting = 340,
        Legal = 350,
        Accounting = 360,
        Marketing = 370,
        ProfessionalServices = 380,

        // Non-profit
        NonProfit = 400,
        Charity = 410,
        ReligiousOrganization = 420
    }

    // ------------------------------------------------------------------------
    // NEW RECORD MODEL (source of truth)
    // ------------------------------------------------------------------------

    public sealed record OrganizationSectorInfo(
        int Id,
        string Code,
        string CodeLower,
        string Name,
        string Description
    )
    {
        public OrganizationSector Enum => (OrganizationSector)Id;
    }

    // ------------------------------------------------------------------------
    // SINGLE SOURCE OF TRUTH
    // ------------------------------------------------------------------------

    private static readonly IReadOnlyList<OrganizationSectorInfo> _all =
    [
        new(0,   "UNKNOWN", "unknown", "Unknown", "Unspecified or unknown sector"),

        // Public
        new(10,  "GOVERNMENT", "government", "Government", "Government entity"),
        new(20,  "DEFENCE", "defence", "Defence", "Military or defence organization"),
        new(30,  "PUBLIC_ADMINISTRATION", "public_administration", "Public Administration", "Public administration body"),

        // Education
        new(40,  "PRIMARY_EDUCATION", "primary_education", "Primary Education", "Primary school"),
        new(50,  "SECONDARY_EDUCATION", "secondary_education", "Secondary Education", "Secondary school"),
        new(60,  "HIGHER_EDUCATION", "higher_education", "Higher Education", "University or tertiary institution"),
        new(70,  "RESEARCH_INSTITUTION", "research_institution", "Research Institution", "Research organization"),

        // Healthcare
        new(80,  "HEALTHCARE", "healthcare", "Healthcare", "Healthcare provider"),
        new(90,  "HOSPITAL", "hospital", "Hospital", "Hospital organization"),
        new(100, "PHARMACEUTICAL", "pharmaceutical", "Pharmaceutical", "Pharmaceutical company"),
        new(110, "BIOTECHNOLOGY", "biotechnology", "Biotechnology", "Biotech company"),

        // Financial
        new(120, "BANKING", "banking", "Banking", "Bank or financial institution"),
        new(130, "INSURANCE", "insurance", "Insurance", "Insurance provider"),
        new(140, "INVESTMENT_MANAGEMENT", "investment_management", "Investment Management", "Investment firm"),
        new(150, "FINTECH", "fintech", "FinTech", "Financial technology company"),

        // Technology
        new(160, "TECHNOLOGY", "technology", "Technology", "Technology company"),
        new(170, "SOFTWARE", "software", "Software", "Software development company"),
        new(180, "SAAS", "saas", "SaaS", "Software-as-a-Service provider"),
        new(190, "TELECOMMUNICATIONS", "telecommunications", "Telecommunications", "Telecommunications provider"),
        new(200, "CYBER_SECURITY", "cyber_security", "Cyber Security", "Cybersecurity organization"),

        // Commercial
        new(210, "MANUFACTURING", "manufacturing", "Manufacturing", "Manufacturing company"),
        new(220, "RETAIL", "retail", "Retail", "Retail business"),
        new(230, "ECOMMERCE", "ecommerce", "E-Commerce", "Online retail business"),
        new(240, "LOGISTICS", "logistics", "Logistics", "Logistics company"),
        new(250, "TRANSPORTATION", "transportation", "Transportation", "Transportation provider"),
        new(260, "ENERGY", "energy", "Energy", "Energy provider"),
        new(270, "MINING", "mining", "Mining", "Mining company"),
        new(280, "CONSTRUCTION", "construction", "Construction", "Construction company"),
        new(290, "REAL_ESTATE", "real_estate", "Real Estate", "Real estate company"),
        new(300, "HOSPITALITY", "hospitality", "Hospitality", "Hospitality business"),
        new(310, "TRAVEL_AND_TOURISM", "travel_and_tourism", "Travel and Tourism", "Travel company"),
        new(320, "MEDIA_AND_ENTERTAINMENT", "media_and_entertainment", "Media and Entertainment", "Media company"),
        new(330, "AGRICULTURE", "agriculture", "Agriculture", "Agricultural organization"),

        // Services
        new(340, "CONSULTING", "consulting", "Consulting", "Consulting firm"),
        new(350, "LEGAL", "legal", "Legal", "Legal services firm"),
        new(360, "ACCOUNTING", "accounting", "Accounting", "Accounting firm"),
        new(370, "MARKETING", "marketing", "Marketing", "Marketing agency"),
        new(380, "PROFESSIONAL_SERVICES", "professional_services", "Professional Services", "Professional services organization"),

        // Nonprofit
        new(400, "NON_PROFIT", "non_profit", "Non-Profit", "Non-profit organization"),
        new(410, "CHARITY", "charity", "Charity", "Charitable organization"),
        new(420, "RELIGIOUS_ORGANIZATION", "religious_organization", "Religious Organization", "Religious institution")
    ];

    public static IReadOnlyList<OrganizationSectorInfo> All => _all;

    // ------------------------------------------------------------------------
    // LOOKUPS
    // ------------------------------------------------------------------------

    public static OrganizationSectorInfo Get(int id)
        => _all.First(x => x.Id == id);

    public static OrganizationSectorInfo Get(OrganizationSector type)
        => Get((int)type);

    public static OrganizationSectorInfo? TryGet(int id)
        => _all.FirstOrDefault(x => x.Id == id);

    public static OrganizationSector ToEnum(int id)
        => (OrganizationSector)id;

    public static int ToId(OrganizationSector type)
        => (int)type;

    // ------------------------------------------------------------------------
    // UI PROJECTION
    // ------------------------------------------------------------------------

    public static IReadOnlyList<(OrganizationSector Type, string Name)> DropdownOptions =>
        _all
            .Select(x => ((OrganizationSector)x.Id, x.Name))
            .ToList();
}