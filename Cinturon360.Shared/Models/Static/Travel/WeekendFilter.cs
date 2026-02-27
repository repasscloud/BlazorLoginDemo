namespace Cinturon360.Shared.Models.Static.Travel;

public enum WeekendFilter : int { ALL_DAYS = 0, NO_SUNDAYS = 1, NO_SATURDAYS = 2, NO_WEEKENDS = 3 }

public static class WeekendFilters
{
    public sealed record Filter(
        WeekendFilter Type, // enum-safe
        string Code,        // "ALL_DAYS"
        string CodeLower,   // "all_days"
        string Label,       // UI label
        string Description  // helper text
    );

    private static readonly IReadOnlyList<Filter> _all =
    [
        new(
            WeekendFilter.ALL_DAYS,
            "ALL_DAYS",
            "all_days",
            "All Days",
            "Include departures on all days of the week."
        ),
        new(
            WeekendFilter.NO_SUNDAYS,
            "NO_SUNDAYS",
            "no_sundays",
            "Exclude Sundays",
            "Exclude departures on Sundays."
        ),
        new(
            WeekendFilter.NO_SATURDAYS,
            "NO_SATURDAYS",
            "no_saturdays",
            "Exclude Saturdays",
            "Exclude departures on Saturdays."
        ),
        new(
            WeekendFilter.NO_WEEKENDS,
            "NO_WEEKENDS",
            "no_weekends",
            "Weekdays Only",
            "Exclude departures on both Saturdays and Sundays."
        )
    ];

    public static IReadOnlyList<Filter> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(f => (f.Code, f.Label)).ToList();

    // Enum-safe lookup
    public static Filter Get(WeekendFilter type) =>
        _all.First(f => f.Type == type);

    // String-safe lookup
    public static Filter? GetByCode(string code) =>
        _all.FirstOrDefault(f =>
            f.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(WeekendFilter type) =>
        Get(type).Code;

    public static WeekendFilter? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
