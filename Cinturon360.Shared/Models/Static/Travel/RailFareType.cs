namespace Cinturon360.Shared.Models.Static.Travel;

public enum RailTravelClassType : int { SECOND = 0, FIRST = 1, BUSINESS = 2, PREMIUM = 3, SLEEPER = 4, CABIN = 5 }

public static class RailFareType
{
    public sealed record Fare(
        RailTravelClassType Type, // enum-safe
        string Code,              // canonical storage code ("SECOND")
        string CodeLower,         // "second"
        string Label,             // "Second"
        string[] Aliases          // supplier / free-text tokens
    );

    // Single source of truth
    private static readonly IReadOnlyList<Fare> _all =
    [
        new(
            RailTravelClassType.SECOND,
            "SECOND",
            "second",
            "Second",
            ["SECOND", "STANDARD", "ECONOMY", "2", "2ND"]
        ),
        new(
            RailTravelClassType.FIRST,
            "FIRST",
            "first",
            "First",
            ["FIRST", "1", "1ST"]
        ),
        new(
            RailTravelClassType.BUSINESS,
            "BUSINESS",
            "business",
            "Business / Executive",
            ["BUSINESS", "EXECUTIVE"]
        ),
        new(
            RailTravelClassType.PREMIUM,
            "PREMIUM",
            "premium",
            "Premium / Comfort",
            ["PREMIUM", "COMFORT", "STANDARD PREMIER"]
        ),
        new(
            RailTravelClassType.SLEEPER,
            "SLEEPER",
            "sleeper",
            "Sleeper / Couchette",
            ["SLEEP", "SLEEPER", "COUCHETTE", "BERTH"]
        ),
        new(
            RailTravelClassType.CABIN,
            "CABIN",
            "cabin",
            "Private Cabin",
            ["CABIN", "COMPARTMENT", "PRIVATE ROOM"]
        )
    ];

    public static IReadOnlyList<Fare> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(f => (f.Code, f.Label)).ToList();

    // Enum-safe lookup
    public static Fare Get(RailTravelClassType type) =>
        _all.First(f => f.Type == type);

    // Canonical + alias lookup
    public static Fare? GetByCode(string code)
    {
        var normalized = code.Trim().ToUpperInvariant();

        return _all.FirstOrDefault(f =>
            f.Code.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
            f.Aliases.Any(a => normalized.Contains(a)));
    }

    // Replacement for old Normalize()
    public static string Normalize(string? text) =>
        ToCode(ToEnum(text) ?? RailTravelClassType.SECOND);

    public static string ToCode(RailTravelClassType type) =>
        Get(type).Code;

    public static RailTravelClassType? ToEnum(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return RailTravelClassType.SECOND;

        var normalized = text.Trim().ToUpperInvariant();

        return _all.FirstOrDefault(f =>
            f.Code.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
            f.Aliases.Any(a => normalized.Contains(a))
        )?.Type;
    }
}
