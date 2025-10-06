namespace BlazorLoginDemo.Shared.Models.Static;

public static class RailFareType
{
    // Universal rail seating classes for policy/UIs.
    public static readonly (string code, string label)[] BookingClassOptions =
    [
        ("SECOND",  "Second"),
        ("FIRST",   "First"),
        ("BUSINESS","Business / Executive"),
        ("PREMIUM", "Premium / Comfort"),  // e.g., “Standard Premier”, “Comfort+”
        ("SLEEPER", "Sleeper / Couchette"),
        ("CABIN",   "Private Cabin")  // whole-room products
    ];

    // Optional: normalize free text from suppliers to one of the codes above.
    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "SECOND";
        var s = text.Trim().ToUpperInvariant();

        // SECOND
        if (s.Contains("SECOND") || s.Contains("STANDARD") || s is "2" or "2ND" || s.Contains("ECONOMY"))
            return "SECOND";

        // FIRST
        if (s.Contains("FIRST") || s is "1" or "1ST")
            return "FIRST";

        // BUSINESS / EXECUTIVE
        if (s.Contains("BUSINESS") || s.Contains("EXECUTIVE"))
            return "BUSINESS";

        // PREMIUM / COMFORT tiers
        if (s.Contains("PREMIUM") || s.Contains("COMFORT") || s.Contains("STANDARD PREMIER"))
            return "PREMIUM";

        // SLEEPER / COUCHETTE / BERTH
        if (s.Contains("SLEEP") || s.Contains("COUCHETTE") || s.Contains("BERTH"))
            return "SLEEPER";

        // Private room products
        if (s.Contains("CABIN") || s.Contains("COMPARTMENT") || s.Contains("PRIVATE ROOM"))
            return "CABIN";

        return "SECOND";
    }
}
