namespace Cinturon360.Shared.Models.Static;

public static class FareType
{
    public static readonly (string code, string label)[] BookingClassOptions =
    [
        ("ECONOMY", "Economy"),
        ("PREMIUM_ECONOMY", "Premium Economy"),
        ("BUSINESS", "Business"),
        ("FIRST", "First Class")
    ];
}
