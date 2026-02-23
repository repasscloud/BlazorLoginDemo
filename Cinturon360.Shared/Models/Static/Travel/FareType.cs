namespace Cinturon360.Shared.Models.Static.Travel;

public enum FlightTravelClassType : int { ECONOMY = 0, PREMIUM_ECONOMY = 1, BUSINESS = 2, FIRST = 3 }
public static class FareType
{
    public sealed record Fare(
        FlightTravelClassType Type, // enum-safe
        string Code,                // "ECONOMY"
        string CodeLower,           // "economy"
        string Label                // "Economy"
    );

    // Single source of truth
    private static readonly IReadOnlyList<Fare> _all =
    [
        new(FlightTravelClassType.ECONOMY,         "ECONOMY",         "economy",         "Economy"),
        new(FlightTravelClassType.PREMIUM_ECONOMY, "PREMIUM_ECONOMY", "premium_economy", "Premium Economy"),
        new(FlightTravelClassType.BUSINESS,        "BUSINESS",        "business",        "Business"),
        new(FlightTravelClassType.FIRST,           "FIRST",           "first",           "First Class")
    ];

    public static IReadOnlyList<Fare> All => _all;

    // UI projection (string-safe for Blazor/InputSelect)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(f => (f.Code, f.Label)).ToList();

    // Enum-safe lookup
    public static Fare Get(FlightTravelClassType type) =>
        _all.First(f => f.Type == type);

    // String-safe lookup (DB / API)
    public static Fare? GetByCode(string code) =>
        _all.FirstOrDefault(f =>
            f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(FlightTravelClassType type) =>
        Get(type).Code;

    public static FlightTravelClassType ToEnum(string code) =>
        GetByCode(code)?.Type
        ?? throw new InvalidOperationException($"Invalid fare code '{code}'.");
}
