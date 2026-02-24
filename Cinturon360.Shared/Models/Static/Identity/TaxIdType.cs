namespace Cinturon360.Shared.Models.Static.Identity;

/// <summary>
/// Represents the type of tax or business registration identifier.
///
/// Naming convention:
/// COUNTRYCODE_IDENTIFIER
///
/// COUNTRYCODE uses ISO 3166-1 alpha-2.
///
/// Regex patterns represent structural validation only.
/// Some identifiers require additional checksum validation.
/// </summary>
public enum TaxIdType
{
    None = 0,

    // =========================================================
    // Australia (AU)
    // =========================================================

    /// <summary>
    /// Australian Business Number (ABN)
    /// Issued by: Australian Taxation Office (ATO)
    /// Length: 11 digits
    /// Regex: ^\d{11}$
    /// Checksum: Yes
    /// PEPPOL Scheme: 0151
    /// Example: 51824753556
    /// </summary>
    AU_ABN,

    /// <summary>
    /// Australian Company Number (ACN)
    /// Issued by: ASIC
    /// Length: 9 digits
    /// Regex: ^\d{9}$
    /// Checksum: Yes
    /// Example: 004085616
    /// </summary>
    AU_ACN,

    /// <summary>
    /// Australian Registered Body Number (ARBN)
    /// Issued by: ASIC
    /// Length: 9 digits
    /// Regex: ^\d{9}$
    /// Checksum: Yes
    /// </summary>
    AU_ARBN,

    /// <summary>
    /// Australian Registered Scheme Number (ARSN)
    /// Issued by: ASIC
    /// Length: 9 digits
    /// Regex: ^\d{9}$
    /// Checksum: Yes
    /// </summary>
    AU_ARSN,


    // =========================================================
    // United States (US)
    // =========================================================

    /// <summary>
    /// Employer Identification Number (EIN)
    /// Issued by: IRS
    /// Length: 9 digits
    /// Regex: ^\d{2}-?\d{7}$
    /// Example: 12-3456789
    /// </summary>
    US_EIN,

    /// <summary>
    /// Taxpayer Identification Number (TIN)
    /// Issued by: IRS
    /// Length: 9 digits
    /// Regex: ^\d{9}$
    /// </summary>
    US_TIN,


    // =========================================================
    // United Kingdom (GB)
    // =========================================================

    /// <summary>
    /// VAT Registration Number
    /// Issued by: HMRC
    /// Regex: ^GB\d{9}(\d{3})?$
    /// Example: GB123456789
    /// </summary>
    GB_VAT,

    /// <summary>
    /// Company Registration Number
    /// Issued by: Companies House
    /// Regex: ^[A-Z0-9]{8}$
    /// Example: 01234567
    /// </summary>
    GB_CRN,


    // =========================================================
    // European Union VAT Numbers
    // =========================================================

    /// <summary>
    /// Austria VAT
    /// Regex: ^ATU\d{8}$
    /// </summary>
    AT_VAT,

    /// <summary>
    /// Belgium VAT
    /// Regex: ^BE0?\d{9}$
    /// </summary>
    BE_VAT,

    /// <summary>
    /// Bulgaria VAT
    /// Regex: ^BG\d{9,10}$
    /// </summary>
    BG_VAT,

    /// <summary>
    /// Croatia VAT
    /// Regex: ^HR\d{11}$
    /// </summary>
    HR_VAT,

    /// <summary>
    /// Cyprus VAT
    /// Regex: ^CY\d{8}[A-Z]$
    /// </summary>
    CY_VAT,

    /// <summary>
    /// Czech Republic VAT
    /// Regex: ^CZ\d{8,10}$
    /// </summary>
    CZ_VAT,

    /// <summary>
    /// Denmark VAT
    /// Regex: ^DK\d{8}$
    /// </summary>
    DK_VAT,

    /// <summary>
    /// Estonia VAT
    /// Regex: ^EE\d{9}$
    /// </summary>
    EE_VAT,

    /// <summary>
    /// Finland VAT
    /// Regex: ^FI\d{8}$
    /// </summary>
    FI_VAT,

    /// <summary>
    /// France VAT
    /// Regex: ^FR[A-HJ-NP-Z0-9]{2}\d{9}$
    /// </summary>
    FR_VAT,

    /// <summary>
    /// Germany VAT
    /// Regex: ^DE\d{9}$
    /// </summary>
    DE_VAT,

    /// <summary>
    /// Greece VAT
    /// Regex: ^EL\d{9}$
    /// </summary>
    GR_VAT,

    /// <summary>
    /// Hungary VAT
    /// Regex: ^HU\d{8}$
    /// </summary>
    HU_VAT,

    /// <summary>
    /// Ireland VAT
    /// Regex: ^IE\d{7}[A-W][A-I0-9]?$
    /// </summary>
    IE_VAT,

    /// <summary>
    /// Italy VAT
    /// Regex: ^IT\d{11}$
    /// </summary>
    IT_VAT,

    /// <summary>
    /// Latvia VAT
    /// Regex: ^LV\d{11}$
    /// </summary>
    LV_VAT,

    /// <summary>
    /// Lithuania VAT
    /// Regex: ^LT(\d{9}|\d{12})$
    /// </summary>
    LT_VAT,

    /// <summary>
    /// Luxembourg VAT
    /// Regex: ^LU\d{8}$
    /// </summary>
    LU_VAT,

    /// <summary>
    /// Malta VAT
    /// Regex: ^MT\d{8}$
    /// </summary>
    MT_VAT,

    /// <summary>
    /// Netherlands VAT
    /// Regex: ^NL\d{9}B\d{2}$
    /// </summary>
    NL_VAT,

    /// <summary>
    /// Poland VAT
    /// Regex: ^PL\d{10}$
    /// </summary>
    PL_VAT,

    /// <summary>
    /// Portugal VAT
    /// Regex: ^PT\d{9}$
    /// </summary>
    PT_VAT,

    /// <summary>
    /// Romania VAT
    /// Regex: ^RO\d{2,10}$
    /// </summary>
    RO_VAT,

    /// <summary>
    /// Slovakia VAT
    /// Regex: ^SK\d{10}$
    /// </summary>
    SK_VAT,

    /// <summary>
    /// Slovenia VAT
    /// Regex: ^SI\d{8}$
    /// </summary>
    SI_VAT,

    /// <summary>
    /// Spain VAT
    /// Regex: ^ES[A-Z0-9]\d{7}[A-Z0-9]$
    /// </summary>
    ES_VAT,

    /// <summary>
    /// Sweden VAT
    /// Regex: ^SE\d{10}01$
    /// </summary>
    SE_VAT,


    // =========================================================
    // New Zealand
    // =========================================================

    /// <summary>
    /// New Zealand Business Number
    /// Regex: ^\d{13}$
    /// </summary>
    NZ_NZBN,

    /// <summary>
    /// New Zealand GST Number
    /// Regex: ^\d{8,9}$
    /// </summary>
    NZ_GST,


    // =========================================================
    // Canada
    // =========================================================

    /// <summary>
    /// Canada Business Number
    /// Regex: ^\d{9}$
    /// </summary>
    CA_BN,

    /// <summary>
    /// Canada GST Number
    /// Regex: ^\d{9}RT\d{4}$
    /// </summary>
    CA_GST,


    // =========================================================
    // Singapore
    // =========================================================

    /// <summary>
    /// Singapore Unique Entity Number
    /// Regex: ^\d{9}[A-Z]$
    /// </summary>
    SG_UEN,


    // =========================================================
    // Generic fallback identifiers
    // =========================================================

    /// <summary>
    /// VAT Identification Number
    /// Used across EU member states.
    /// Used for intra-EU trade and VAT reporting.
    /// </summary>
    EU_VAT,

    /// <summary>
    /// Generic VAT identifier
    /// Regex varies by country
    /// </summary>
    VAT,

    /// <summary>
    /// Generic Tax Identifier
    /// </summary>
    TIN,

    /// <summary>
    /// Generic Business Registration Number
    /// </summary>
    BRN
}