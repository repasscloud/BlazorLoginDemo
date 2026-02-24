namespace Cinturon360.Shared.Models.Static.Platform;
public enum OrganizationClassification : int
{
    Unknown = 0,

    // --- Size progression (auto-upgradable) ---

    /// <summary>1–5 licensed users</summary>
    PreStartup = 10,

    /// <summary>6–25 licensed users</summary>
    Startup = 20,

    /// <summary>26–75 licensed users</summary>
    ScaleUp = 30,

    /// <summary>1–10 licensed users</summary>
    MicroBusiness = 40,

    /// <summary>11–50 licensed users</summary>
    SmallBusiness = 50,

    /// <summary>51–250 licensed users</summary>
    LowerMidMarket = 60,

    /// <summary>251–1000 licensed users</summary>
    UpperMidMarket = 70,

    /// <summary>1001–5000 licensed users</summary>
    Corporate = 80,

    /// <summary>5001–20000 licensed users</summary>
    Enterprise = 90,

    /// <summary>20000+ licensed users</summary>
    GlobalEnterprise = 100,

    // --- Special identity (manual override only) ---

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