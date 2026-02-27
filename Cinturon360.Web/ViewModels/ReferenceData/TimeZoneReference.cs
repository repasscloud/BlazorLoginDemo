namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record TimeZoneOption(
    int Id,        // 7011
    string IanaId  // "Australia/Adelaide"
);

public sealed record TimeZoneReference
{
    public IReadOnlyList<TimeZoneOption> Options { get; init; } = [];
}
