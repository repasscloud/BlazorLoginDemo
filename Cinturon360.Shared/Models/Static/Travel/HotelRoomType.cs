namespace Cinturon360.Shared.Models.Static.Travel;

public enum HotelRoomClassType : int { STANDARD = 0, SUPERIOR = 1, DELUXE = 2, EXECUTIVE = 3, CLUB = 4 }

public static class HotelRoomType
{
    public sealed record Room(
        HotelRoomClassType Type, // enum-safe
        string Code,             // canonical storage code (STANDARD)
        string CodeLower,        // standard
        string Label,            // Standard
        string[] Aliases         // ["STD", "STANDARD"]
    );

    // Single source of truth
    private static readonly IReadOnlyList<Room> _all =
    [
        new(
            HotelRoomClassType.STANDARD,
            "STANDARD",
            "standard",
            "Standard",
            ["STD", "STANDARD"]
        ),
        new(
            HotelRoomClassType.SUPERIOR,
            "SUPERIOR",
            "superior",
            "Superior",
            ["SUP", "SUPERIOR"]
        ),
        new(
            HotelRoomClassType.DELUXE,
            "DELUXE",
            "deluxe",
            "Deluxe",
            ["DLX", "DELUXE"]
        ),
        new(
            HotelRoomClassType.EXECUTIVE,
            "EXECUTIVE",
            "executive",
            "Executive",
            ["EXEC", "EXECUTIVE"]
        ),
        new(
            HotelRoomClassType.CLUB,
            "CLUB",
            "club",
            "Club",
            ["CLUB"]
        )
    ];

    public static IReadOnlyList<Room> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(r => (r.Code, r.Label)).ToList();

    // Enum-safe lookup
    public static Room Get(HotelRoomClassType type) =>
        _all.First(r => r.Type == type);

    // String-safe lookup (canonical + aliases)
    public static Room? GetByCode(string code)
    {
        var normalized = code.Trim().ToUpperInvariant();

        return _all.FirstOrDefault(r =>
            r.Code.Equals(normalized, StringComparison.OrdinalIgnoreCase) ||
            r.Aliases.Any(a => a.Equals(normalized, StringComparison.OrdinalIgnoreCase)));
    }

    // Helpers
    public static string ToCode(HotelRoomClassType type) =>
        Get(type).Code;

    public static HotelRoomClassType ToEnum(string code) =>
        GetByCode(code)?.Type
        ?? throw new InvalidOperationException($"Invalid hotel room type code '{code}'.");
}
