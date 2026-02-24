namespace Cinturon360.Shared.Models.Static.Identity;

public enum TaxationType : int
{
    None = 0,

    AU_ABN,
    AU_ACN,
    AU_ARBN,
    AU_ARSN,

    US_EIN,
    US_TIN,

    GB_VAT,
    GB_CRN,

    AT_VAT,
    BE_VAT,
    BG_VAT,
    HR_VAT,
    CY_VAT,
    CZ_VAT,
    DK_VAT,
    EE_VAT,
    FI_VAT,
    FR_VAT,
    DE_VAT,
    GR_VAT,
    HU_VAT,
    IE_VAT,
    IT_VAT,
    LV_VAT,
    LT_VAT,
    LU_VAT,
    MT_VAT,
    NL_VAT,
    PL_VAT,
    PT_VAT,
    RO_VAT,
    SK_VAT,
    SI_VAT,
    ES_VAT,
    SE_VAT,

    NZ_NZBN,
    NZ_GST,

    CA_BN,
    CA_GST,

    SG_UEN,

    EU_VAT,
    VAT,
    TIN,
    BRN
}

public static class TaxationTypeCatalog
{
    public sealed record TaxId(
        TaxationType Type,
        string Code,
        string CodeLower,
        string CountryIso2,
        string Label,
        string Regex,
        string Description
    );

    private static readonly IReadOnlyList<TaxId> _all =
    [
        new(TaxationType.AU_ABN, "AU_ABN", "au_abn", "AU", "Australian Business Number", @"^\d{11}$", "Primary Australian business identifier."),
        new(TaxationType.AU_ACN, "AU_ACN", "au_acn", "AU", "Australian Company Number", @"^\d{9}$", "Company identifier issued by ASIC."),
        new(TaxationType.AU_ARBN, "AU_ARBN", "au_arbn", "AU", "Australian Registered Body Number", @"^\d{9}$", "Foreign company identifier."),
        new(TaxationType.AU_ARSN, "AU_ARSN", "au_arsn", "AU", "Australian Registered Scheme Number", @"^\d{9}$", "Managed investment scheme identifier."),

        new(TaxationType.US_EIN, "US_EIN", "us_ein", "US", "Employer Identification Number", @"^\d{2}-?\d{7}$", "US business tax identifier."),
        new(TaxationType.US_TIN, "US_TIN", "us_tin", "US", "Taxpayer Identification Number", @"^\d{9}$", "US general tax identifier."),

        new(TaxationType.GB_VAT, "GB_VAT", "gb_vat", "GB", "VAT Registration Number", @"^GB\d{9}(\d{3})?$", "UK VAT identifier."),
        new(TaxationType.GB_CRN, "GB_CRN", "gb_crn", "GB", "Company Registration Number", @"^[A-Z0-9]{8}$", "UK company identifier."),

        new(TaxationType.AT_VAT, "AT_VAT", "at_vat", "AT", "Austria VAT Number", @"^ATU\d{8}$", "Austria VAT identifier."),
        new(TaxationType.BE_VAT, "BE_VAT", "be_vat", "BE", "Belgium VAT Number", @"^BE0?\d{9}$", "Belgium VAT identifier."),
        new(TaxationType.BG_VAT, "BG_VAT", "bg_vat", "BG", "Bulgaria VAT Number", @"^BG\d{9,10}$", "Bulgaria VAT identifier."),
        new(TaxationType.HR_VAT, "HR_VAT", "hr_vat", "HR", "Croatia VAT Number", @"^HR\d{11}$", "Croatia VAT identifier."),
        new(TaxationType.CY_VAT, "CY_VAT", "cy_vat", "CY", "Cyprus VAT Number", @"^CY\d{8}[A-Z]$", "Cyprus VAT identifier."),
        new(TaxationType.CZ_VAT, "CZ_VAT", "cz_vat", "CZ", "Czech Republic VAT Number", @"^CZ\d{8,10}$", "Czech VAT identifier."),
        new(TaxationType.DK_VAT, "DK_VAT", "dk_vat", "DK", "Denmark VAT Number", @"^DK\d{8}$", "Denmark VAT identifier."),
        new(TaxationType.EE_VAT, "EE_VAT", "ee_vat", "EE", "Estonia VAT Number", @"^EE\d{9}$", "Estonia VAT identifier."),
        new(TaxationType.FI_VAT, "FI_VAT", "fi_vat", "FI", "Finland VAT Number", @"^FI\d{8}$", "Finland VAT identifier."),
        new(TaxationType.FR_VAT, "FR_VAT", "fr_vat", "FR", "France VAT Number", @"^FR[A-HJ-NP-Z0-9]{2}\d{9}$", "France VAT identifier."),
        new(TaxationType.DE_VAT, "DE_VAT", "de_vat", "DE", "Germany VAT Number", @"^DE\d{9}$", "Germany VAT identifier."),
        new(TaxationType.GR_VAT, "GR_VAT", "gr_vat", "GR", "Greece VAT Number", @"^EL\d{9}$", "Greece VAT identifier."),
        new(TaxationType.HU_VAT, "HU_VAT", "hu_vat", "HU", "Hungary VAT Number", @"^HU\d{8}$", "Hungary VAT identifier."),
        new(TaxationType.IE_VAT, "IE_VAT", "ie_vat", "IE", "Ireland VAT Number", @"^IE\d{7}[A-W][A-I0-9]?$", "Ireland VAT identifier."),
        new(TaxationType.IT_VAT, "IT_VAT", "it_vat", "IT", "Italy VAT Number", @"^IT\d{11}$", "Italy VAT identifier."),
        new(TaxationType.LV_VAT, "LV_VAT", "lv_vat", "LV", "Latvia VAT Number", @"^LV\d{11}$", "Latvia VAT identifier."),
        new(TaxationType.LT_VAT, "LT_VAT", "lt_vat", "LT", "Lithuania VAT Number", @"^LT(\d{9}|\d{12})$", "Lithuania VAT identifier."),
        new(TaxationType.LU_VAT, "LU_VAT", "lu_vat", "LU", "Luxembourg VAT Number", @"^LU\d{8}$", "Luxembourg VAT identifier."),
        new(TaxationType.MT_VAT, "MT_VAT", "mt_vat", "MT", "Malta VAT Number", @"^MT\d{8}$", "Malta VAT identifier."),
        new(TaxationType.NL_VAT, "NL_VAT", "nl_vat", "NL", "Netherlands VAT Number", @"^NL\d{9}B\d{2}$", "Netherlands VAT identifier."),
        new(TaxationType.PL_VAT, "PL_VAT", "pl_vat", "PL", "Poland VAT Number", @"^PL\d{10}$", "Poland VAT identifier."),
        new(TaxationType.PT_VAT, "PT_VAT", "pt_vat", "PT", "Portugal VAT Number", @"^PT\d{9}$", "Portugal VAT identifier."),
        new(TaxationType.RO_VAT, "RO_VAT", "ro_vat", "RO", "Romania VAT Number", @"^RO\d{2,10}$", "Romania VAT identifier."),
        new(TaxationType.SK_VAT, "SK_VAT", "sk_vat", "SK", "Slovakia VAT Number", @"^SK\d{10}$", "Slovakia VAT identifier."),
        new(TaxationType.SI_VAT, "SI_VAT", "si_vat", "SI", "Slovenia VAT Number", @"^SI\d{8}$", "Slovenia VAT identifier."),
        new(TaxationType.ES_VAT, "ES_VAT", "es_vat", "ES", "Spain VAT Number", @"^ES[A-Z0-9]\d{7}[A-Z0-9]$", "Spain VAT identifier."),
        new(TaxationType.SE_VAT, "SE_VAT", "se_vat", "SE", "Sweden VAT Number", @"^SE\d{10}01$", "Sweden VAT identifier."),

        new(TaxationType.NZ_NZBN, "NZ_NZBN", "nz_nzbn", "NZ", "New Zealand Business Number", @"^\d{13}$", "New Zealand business identifier."),
        new(TaxationType.NZ_GST, "NZ_GST", "nz_gst", "NZ", "New Zealand GST Number", @"^\d{8,9}$", "New Zealand GST identifier."),

        new(TaxationType.CA_BN, "CA_BN", "ca_bn", "CA", "Canada Business Number", @"^\d{9}$", "Canada business identifier."),
        new(TaxationType.CA_GST, "CA_GST", "ca_gst", "CA", "Canada GST Number", @"^\d{9}RT\d{4}$", "Canada GST identifier."),

        new(TaxationType.SG_UEN, "SG_UEN", "sg_uen", "SG", "Singapore Unique Entity Number", @"^\d{9}[A-Z]$", "Singapore business identifier."),

        new(TaxationType.EU_VAT, "EU_VAT", "eu_vat", "EU", "EU VAT Number", "", "Generic EU VAT identifier."),
        new(TaxationType.VAT, "VAT", "vat", "INT", "VAT Number", "", "Generic VAT identifier."),
        new(TaxationType.TIN, "TIN", "tin", "INT", "Tax Identification Number", "", "Generic tax identifier."),
        new(TaxationType.BRN, "BRN", "brn", "INT", "Business Registration Number", "", "Generic business identifier.")
    ];

    public static IReadOnlyList<TaxId> All => _all;

    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(x => (x.Code, x.Label)).ToList();

    public static TaxId Get(TaxationType type) =>
        _all.First(x => x.Type == type);

    public static TaxId? GetByCode(string code) =>
        _all.FirstOrDefault(x =>
            x.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    public static string ToCode(TaxationType type) =>
        Get(type).Code;

    public static TaxationType? ToEnum(string code) =>
        GetByCode(code)?.Type;
}