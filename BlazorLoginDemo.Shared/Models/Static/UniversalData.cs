namespace BlazorLoginDemo.Shared.Models.Static;

public static class UniversalData
{
    public static Dictionary<string, string> BookingCurrencyOptions = new Dictionary<string, string>()
    {
        { "🇦🇺 Australian Dollar", "AUD" },
        { "🇪🇺 Euro", "EUR" },
        { "🇺🇸 US Dollar", "USD" },
        { "🇬🇧 Pound Sterling", "GBP" },
        { "🇨🇦 Canadian Dollar", "CAD" },
        { "🇨🇭 Swiss Franc", "CHF" },
        { "🇨🇳 Chinese Renminbi", "CNH" },
        { "🇭🇰 Hong Kong Dollar", "HKD" },
        { "🇳🇿 New Zealand Dollar", "NZD" }
    };

    public static readonly (string code, string label)[] CurrencyFullNameOptions =
    [
        ("AUD", "Australian Dollar 🇦🇺"),
        ("EUR", "Euro 🇪🇺"),
        ("USD", "US Dollar 🇺🇸"),
        ("GBP", "Pound Sterling 🇬🇧"),
        ("CAD", "Canadian Dollar 🇨🇦"),
        ("CHF", "Swiss Franc 🇨🇭"),
        ("CNH", "Chinese Renminbi 🇨🇳"),
        ("HKD", "Hong Kong Dollar 🇭🇰"),
        ("NZD", "New Zealand Dollar 🇳🇿")
    ];

    public record CurrencyOption(string Code, string Flag);
    public static readonly CurrencyOption[] _currencyOptions =
    [
        new CurrencyOption(Code: "AUD", Flag: "🇦🇺"),
        new CurrencyOption(Code: "GBP", Flag: "🇬🇧"),
        new CurrencyOption(Code: "USD", Flag: "🇺🇸"),
        new CurrencyOption(Code: "NZD", Flag: "🇳🇿"),
        new CurrencyOption(Code: "CAD", Flag: "🇨🇦"),
        new CurrencyOption(Code: "HKD", Flag: "🇭🇰"),
        new CurrencyOption(Code: "EUR", Flag: "🇪🇺"),
    ];
}
