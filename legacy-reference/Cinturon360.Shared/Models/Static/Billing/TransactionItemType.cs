namespace Cinturon360.Shared.Models.Static.Billing;

public enum TransactionItemType : int { NONE = 0, FLIGHT = 1, HOTEL = 2, CAR = 3, RAIL = 4, TRANSFER = 5, ACTIVITY = 6, ESIM = 7 }

public static class TransactionItemTypes
{
    public sealed record ItemType(
        TransactionItemType Type,
        string Code,
        string CodeLower,
        string Label,
        string Description
    );

    private static readonly IReadOnlyList<ItemType> _all =
    [
        new(TransactionItemType.NONE,      "NONE",      "none",      "None",      "No billable transaction item."),
        new(TransactionItemType.FLIGHT,    "FLIGHT",    "flight",    "Flight",    "Flight booking or airfare-related charge."),
        new(TransactionItemType.HOTEL,     "HOTEL",     "hotel",     "Hotel",     "Hotel accommodation booking."),
        new(TransactionItemType.CAR,       "CAR",       "car",       "Car",       "Car rental booking."),
        new(TransactionItemType.RAIL,      "RAIL",      "rail",      "Rail",      "Rail or train booking."),
        new(TransactionItemType.TRANSFER,  "TRANSFER",  "transfer",  "Transfer",  "Ground transfer or shuttle service."),
        new(TransactionItemType.ACTIVITY,  "ACTIVITY",  "activity",  "Activity",  "Tours, events, or other activities."),
        new(TransactionItemType.ESIM,      "ESIM",      "esim",      "eSIM",      "Digital SIM or mobile data plan.")
    ];

    public static IReadOnlyList<ItemType> All => _all;

    // UI projection (dropdown-safe)
    public static IReadOnlyList<(string Code, string Label)> DropdownOptions =>
        _all.Select(i => (i.Code, i.Label)).ToList();

    // Enum-safe lookup
    public static ItemType Get(TransactionItemType type) =>
        _all.First(i => i.Type == type);

    // String-safe lookup (DB / API)
    public static ItemType? GetByCode(string code) =>
        _all.FirstOrDefault(i =>
            i.Code.Equals(code, StringComparison.OrdinalIgnoreCase));

    // Helpers
    public static string ToCode(TransactionItemType type) =>
        Get(type).Code;

    public static TransactionItemType? ToEnum(string code) =>
        GetByCode(code)?.Type;
}
