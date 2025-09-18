namespace BlazorLoginDemo.Shared.Models.Static;

public static class CabinClassCoverages
{
    public static Dictionary<string, string> CabinClassCoverageOptions { get; set; } = new Dictionary<string, string>()
    {
        { "Most segments", "MOST_SEGMENTS" },
        { "At least one segment", "AT_LEAST_ONE_SEGMENT" },
        { "All segments", "ALL_SEGMENTS" }
    };
}
