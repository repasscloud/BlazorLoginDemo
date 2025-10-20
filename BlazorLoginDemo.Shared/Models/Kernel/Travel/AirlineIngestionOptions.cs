namespace BlazorLoginDemo.Shared.Models.Kernel.Travel;

public sealed class AirlineIngestionOptions
{
    public const string SectionName = "AirlineIngestion";
    public string SourceUrl { get; init; } = "";
    public string HttpClientName { get; init; } = "airlines";
}