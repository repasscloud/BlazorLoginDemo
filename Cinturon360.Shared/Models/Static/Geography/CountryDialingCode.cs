using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Static.Geography;

public enum CountryDialingCode
{
    // -------------------------
    // Common / primary markets
    // -------------------------

    [Display(Name = "Australia")]
    Australia = 61,            // AUS / AU

    [Display(Name = "United States")]
    UnitedStates = 1,          // USA / US

    [Display(Name = "United Kingdom")]
    UnitedKingdom = 44,        // GBR / GB

    [Display(Name = "Canada")]
    Canada = 1,                // CAN / CA

    [Display(Name = "New Zealand")]
    NewZealand = 64,           // NZL / NZ

    [Display(Name = "Switzerland")]
    Switzerland = 41,          // CHE / CH

    [Display(Name = "Hong Kong")]
    HongKong = 852,            // HKG / HK

    [Display(Name = "Singapore")]
    Singapore = 65,            // SGP / SG

    [Display(Name = "Japan")]
    Japan = 81,                // JPN / JP

    [Display(Name = "South Korea")]
    SouthKorea = 82,           // KOR / KR

    // -------------------------
    // Eurozone / major Europe
    // -------------------------

    [Display(Name = "Germany")]
    Germany = 49,              // DEU / DE

    [Display(Name = "France")]
    France = 33,               // FRA / FR

    [Display(Name = "Italy")]
    Italy = 39,                // ITA / IT

    [Display(Name = "Spain")]
    Spain = 34,                // ESP / ES

    [Display(Name = "Netherlands")]
    Netherlands = 31,          // NLD / NL

    [Display(Name = "Belgium")]
    Belgium = 32,              // BEL / BE

    [Display(Name = "Austria")]
    Austria = 43,              // AUT / AT

    [Display(Name = "Ireland")]
    Ireland = 353,             // IRL / IE

    [Display(Name = "Sweden")]
    Sweden = 46,               // SWE / SE

    [Display(Name = "Norway")]
    Norway = 47,               // NOR / NO

    [Display(Name = "Denmark")]
    Denmark = 45,              // DNK / DK

    // -------------------------
    // Asia (non-primary)
    // -------------------------

    [Display(Name = "China")]
    China = 86,                // CHN / CN

    [Display(Name = "India")]
    India = 91,                // IND / IN

    [Display(Name = "Armenia")]
    Armenia = 374,             // ARM / AM

    // -------------------------
    // Americas (non-primary)
    // -------------------------

    [Display(Name = "Brazil")]
    Brazil = 55,               // BRA / BR

    [Display(Name = "Argentina")]
    Argentina = 54,            // ARG / AR

    [Display(Name = "Antigua and Barbuda")]
    AntiguaAndBarbuda = 1,     // ATG / AG

    // -------------------------
    // Africa / Middle East
    // -------------------------

    [Display(Name = "South Africa")]
    SouthAfrica = 27,          // ZAF / ZA

    [Display(Name = "Algeria")]
    Algeria = 213,             // DZA / DZ

    [Display(Name = "Angola")]
    Angola = 244,              // AGO / AO

    // -------------------------
    // Smaller / less common
    // -------------------------

    [Display(Name = "Afghanistan")]
    Afghanistan = 93,          // AFG / AF

    [Display(Name = "Albania")]
    Albania = 355,             // ALB / AL

    [Display(Name = "Andorra")]
    Andorra = 376              // AND / AD
}
