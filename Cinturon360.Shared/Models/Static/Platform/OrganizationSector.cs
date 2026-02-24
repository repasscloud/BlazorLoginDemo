namespace Cinturon360.Shared.Models.Static.Platform;
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