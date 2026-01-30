namespace Cinturon360.Shared.Models.Static.Travel;

public enum CabinClassCoverageType : int { MOST_SEGMENTS = 0, AT_LEAST_ONE_SEGMENT = 1, ALL_SEGMENTS = 2 }

public static class CabinClassCoverages
{
    public sealed record Coverage(
        CabinClassCoverageType Type, // enum-safe
        string Code,                 // "MOST_SEGMENTS"
        string CodeLower,            // "most_segments"
        string Label,                // UI label
        string Description           // helper text / tooltip
    );

    // Single source of truth
    private static readonly IReadOnlyList<Coverage> _all =
    [
        new(
            CabinClassCoverageType.MOST_SEGMENTS,
            "MOST_SEGMENTS",
            "most_segments",
            "Most Segments",
            "Applies the cabin class to the majority of trip segments."
        ),
        new(
            CabinClassCoverageType.AT_LEAST_ONE_SEGMENT,
            "AT_LEAST_ONE_SEGMENT",
            "at_least_one_segment",
            "1+ Segment",
            "Requires at least one segment to match the selected cabin class."
        ),
        new(
            CabinClassCoverageType.ALL_SEGMENTS,
            "ALL_SEGMENTS",
            "all_segments",
            "All Segments",
            "Requires every segment in the trip to match the selected cabin class."
        )
    ];

    public static IReadOnlyList<Coverage> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(c => (c.Code, c.Label)).ToList();

    // Enum-safe lookup
    public static Coverage Get(CabinClassCoverageType type) =>
        _all.First(c => c.Type == type);

    // String-safe lookup (DB / API)
    public static Coverage? GetByCode(string code) =>
        _all.FirstOrDefault(c =>
            c.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(CabinClassCoverageType type) =>
        Get(type).Code;

    public static CabinClassCoverageType? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
