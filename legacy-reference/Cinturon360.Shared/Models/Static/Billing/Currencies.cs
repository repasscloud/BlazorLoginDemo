namespace Cinturon360.Shared.Models.Static.Billing;

public static partial class Currencies
{
    public sealed record Currency(
        string Code,            // AUD
        string CodeLower,       // aud
        string Name,            // Australian Dollar
        string Flag,            // 🇦🇺
        string Symbol,          // $
        string LetterSymbol     // A$
    );

    // Single source of truth
    private static readonly IReadOnlyList<Currency> _all =
    [
        // Major
        new("AUD", "aud", "Australian Dollar", "🇦🇺", "$",   "A$"),
        new("USD", "usd", "US Dollar",         "🇺🇸", "$",   "US$"),
        new("EUR", "eur", "Euro",              "🇪🇺", "€",   "€"),
        new("GBP", "gbp", "Pound Sterling",    "🇬🇧", "£",   "£"),
        new("CAD", "cad", "Canadian Dollar",   "🇨🇦", "$",   "C$"),
        new("NZD", "nzd", "New Zealand Dollar","🇳🇿", "$",   "NZ$"),
        new("CHF", "chf", "Swiss Franc",       "🇨🇭", "CHF", "CHF"),
        new("HKD", "hkd", "Hong Kong Dollar",  "🇭🇰", "$",   "HK$"),
        new("SGD", "sgd", "Singapore Dollar",  "🇸🇬", "$",   "S$"),
        new("JPY", "jpy", "Japanese Yen",      "🇯🇵", "¥",   "¥"),
        new("KRW", "krw", "South Korean Won",  "🇰🇷", "₩",   "₩"),

        // Asia
        new("CNY", "cny", "Chinese Yuan",      "🇨🇳", "¥",  "CN¥"),
        new("INR", "inr", "Indian Rupee",      "🇮🇳", "₹",  "₹"),
        new("THB", "thb", "Thai Baht",         "🇹🇭", "฿",  "฿"),
        new("MYR", "myr", "Malaysian Ringgit", "🇲🇾", "RM", "RM"),
        new("IDR", "idr", "Indonesian Rupiah", "🇮🇩", "Rp", "Rp"),
        new("PHP", "php", "Philippine Peso",   "🇵🇭", "₱",  "₱"),
        new("VND", "vnd", "Vietnamese Dong",   "🇻🇳", "₫",  "₫"),

        // Europe
        new("SEK", "sek", "Swedish Krona",     "🇸🇪", "kr",  "kr"),
        new("NOK", "nok", "Norwegian Krone",   "🇳🇴", "kr",  "kr"),
        new("DKK", "dkk", "Danish Krone",      "🇩🇰", "kr",  "kr"),
        new("PLN", "pln", "Polish Zloty",      "🇵🇱", "zł",  "zł"),
        new("CZK", "czk", "Czech Koruna",      "🇨🇿", "Kč",  "Kč"),
        new("HUF", "huf", "Hungarian Forint",  "🇭🇺", "Ft",  "Ft"),
        new("RON", "ron", "Romanian Leu",      "🇷🇴", "lei", "lei"),
        new("BGN", "bgn", "Bulgarian Lev",     "🇧🇬", "лв",  "лв"),

        // Americas
        new("MXN", "mxn", "Mexican Peso",      "🇲🇽", "$",  "MX$"),
        new("BRL", "brl", "Brazilian Real",    "🇧🇷", "R$", "R$"),
        new("CLP", "clp", "Chilean Peso",      "🇨🇱", "$",  "CL$"),
        new("COP", "cop", "Colombian Peso",    "🇨🇴", "$",  "CO$"),
        new("PEN", "pen", "Peruvian Sol",      "🇵🇪", "S/", "S/"),

        // Middle East / Africa
        new("ILS", "ils", "Israeli New Shekel", "🇮🇱", "₪",   "₪"),
        new("ZAR", "zar", "South African Rand", "🇿🇦", "R",   "R"),
        new("AED", "aed", "UAE Dirham",         "🇦🇪", "د.إ", "د.إ"),
        new("SAR", "sar", "Saudi Riyal",        "🇸🇦", "﷼",   "﷼")
    ];

    public static IReadOnlyList<Currency> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Name)> DropdownOptions =>
        _all.Select(c => (c.Code, c.Name)).ToList();
}
