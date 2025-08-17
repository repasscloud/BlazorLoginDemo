namespace BlazorLoginDemo.Shared.Models.Static;

public static class FareTypes
{
    public static Dictionary<string, string> AirfaresBookingClassOptions { get; set; } = new Dictionary<string, string>()
    {
        { "Economy 💰", "ECONOMY" },
        { "Premium Economy 💰💰", "PREMIUM_ECONOMY" },
        { "Business 💰💰💰", "BUSINESS" },
        { "First 💰💰💰💰", "FIRST"}
    };

    public static Dictionary<string, string> AirfaresBookingClassOptionsMaxEconomy { get; set; } = new Dictionary<string, string>()
    {
        { "Economy", "ECONOMY" },
    };

    public static Dictionary<string, string> AirfaresBookingClassOptionsMaxPremiumEconomy { get; set; } = new Dictionary<string, string>()
    {
        { "Economy", "ECONOMY" },
        { "Premium Economy", "PREMIUM_ECONOMY" },
    };

    public static Dictionary<string, string> AirfaresBookingClassOptionsMaxBusiness { get; set; } = new Dictionary<string, string>()
    {
        { "Economy", "ECONOMY" },
        { "Premium Economy", "PREMIUM_ECONOMY" },
        { "Business", "BUSINESS" },
    };

    public static Dictionary<string, string> AirfaresBookingClassOptionsMaxFirst { get; set; } = new Dictionary<string, string>()
    {
        { "Economy", "ECONOMY" },
        { "Premium Economy", "PREMIUM_ECONOMY" },
        { "Business", "BUSINESS" },
        { "First", "FIRST"}
    };
}
