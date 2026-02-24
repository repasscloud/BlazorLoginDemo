namespace Cinturon360.Shared.Models.Static.Platform;

public enum OrganizationType : int
{
    sudo = 0,

    // Commercial reseller chain
    MasterVendor = 10,     // Global master license holder
    RegionalVendor = 20,   // e.g. APAC / EMEA
    CountryVendor = 30,    // e.g. AU / NZ / US
    FranchiseVendor = 40,  // Sub-licensed entity

    // Operational layer
    Tmc = 100,    // Travel Management Company
    Client = 200  // End customer organization
}