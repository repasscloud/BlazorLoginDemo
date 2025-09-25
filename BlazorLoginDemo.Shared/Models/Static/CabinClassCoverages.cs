namespace BlazorLoginDemo.Shared.Models.Static;

public static class CabinClassCoverages
{
    public static readonly (string code, string label)[] CabinClassCoverageOptions =
    [
        ("MOST_SEGMENTS", "Most Segments"),
        ("AT_LEAST_ONE_SEGMENT", "Min 1 Segment"),
        ("ALL_SEGMENTS", "All Segments"),
    ];
}
