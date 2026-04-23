namespace Cinturon360.Shared.Models.Static.Organization;

/// <summary>
/// Static reference data for organization types.
/// Record model is the source of truth.
/// Enum exists for legacy compatibility only.
/// </summary>
public static partial class OrganizationTypes
{
    // ------------------------------------------------------------------------
    // LEGACY ENUM (do NOT remove — used by existing aggregates, DB mappings)
    // ------------------------------------------------------------------------

    public enum OrganizationType : int
    {
        sudo = 0,

        // Commercial reseller chain
        MasterVendor = 10,
        RegionalVendor = 20,
        CountryVendor = 30,
        FranchiseVendor = 40,

        // Operational layer
        Tmc = 100,
        Client = 200
    }

    // ------------------------------------------------------------------------
    // NEW RECORD MODEL (source of truth)
    // ------------------------------------------------------------------------

    public sealed record OrganizationTypeInfo(
        int Id,
        OrganizationType Type,
        string Code,
        string CodeLower,
        string Name,
        string Description
    )
    {
        public OrganizationType Enum => (OrganizationType)Id;
    }

    // ------------------------------------------------------------------------
    // SINGLE SOURCE OF TRUTH
    // ------------------------------------------------------------------------

    private static readonly IReadOnlyList<OrganizationTypeInfo> _all =
    [
        new(
            Id: 0,
            Type: OrganizationType.sudo,
            Code: "SUDO",
            CodeLower: "sudo",
            Name: "System",
            Description: "Internal system-level organization"
        ),

        new(
            Id: 10,
            Type: OrganizationType.MasterVendor,
            Code: "MASTER_VENDOR",
            CodeLower: "master_vendor",
            Name: "Master Vendor",
            Description: "Global master license holder"
        ),

        new(
            Id: 20,
            Type: OrganizationType.RegionalVendor,
            Code: "REGIONAL_VENDOR",
            CodeLower: "regional_vendor",
            Name: "Regional Vendor",
            Description: "Regional license holder"
        ),

        new(
            Id: 30,
            Type: OrganizationType.CountryVendor,
            Code: "COUNTRY_VENDOR",
            CodeLower: "country_vendor",
            Name: "Country Vendor",
            Description: "Country-level license holder"
        ),

        new(
            Id: 40,
            Type: OrganizationType.FranchiseVendor,
            Code: "FRANCHISE_VENDOR",
            CodeLower: "franchise_vendor",
            Name: "Franchise Vendor",
            Description: "Sub-licensed entity"
        ),

        new(
            Id: 100,
            Type: OrganizationType.Tmc,
            Code: "TMC",
            CodeLower: "tmc",
            Name: "Travel Management Company",
            Description: "Travel Management Company"
        ),

        new(
            Id: 200,
            Type: OrganizationType.Client,
            Code: "CLIENT",
            CodeLower: "client",
            Name: "Client",
            Description: "End customer organization"
        )
    ];

    public static IReadOnlyList<OrganizationTypeInfo> All => _all;

    // ------------------------------------------------------------------------
    // LOOKUPS
    // ------------------------------------------------------------------------

    public static OrganizationTypeInfo Get(int id)
        => _all.First(x => x.Id == id);

    public static OrganizationTypeInfo Get(OrganizationType type)
        => Get((int)type);

    public static OrganizationTypeInfo? TryGet(int id)
        => _all.FirstOrDefault(x => x.Id == id);

    // ------------------------------------------------------------------------
    // CONVERSION HELPERS
    // ------------------------------------------------------------------------

    public static OrganizationType ToEnum(int id)
        => (OrganizationType)id;

    public static int ToId(OrganizationType type)
        => (int)type;

    // ------------------------------------------------------------------------
    // STRONGLY TYPED ACCESSORS
    // ------------------------------------------------------------------------

    public static OrganizationTypeInfo Sudo            => Get(OrganizationType.sudo);
    public static OrganizationTypeInfo MasterVendor    => Get(OrganizationType.MasterVendor);
    public static OrganizationTypeInfo RegionalVendor  => Get(OrganizationType.RegionalVendor);
    public static OrganizationTypeInfo CountryVendor   => Get(OrganizationType.CountryVendor);
    public static OrganizationTypeInfo FranchiseVendor => Get(OrganizationType.FranchiseVendor);
    public static OrganizationTypeInfo Tmc             => Get(OrganizationType.Tmc);
    public static OrganizationTypeInfo Client          => Get(OrganizationType.Client);

    // ------------------------------------------------------------------------
    // UI PROJECTION
    // ------------------------------------------------------------------------

    public static IReadOnlyList<(OrganizationType Type, string Name)> DropdownOptions =>
        _all
            .Where(x => x.Id != 0)
            .Select(x => ((OrganizationType)x.Id, x.Name))
            .ToList();
}