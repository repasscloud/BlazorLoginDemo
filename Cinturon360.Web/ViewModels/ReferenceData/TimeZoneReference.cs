namespace Cinturon360.Web.ViewModels.ReferenceData;

public sealed record TimeZoneReference(
    IReadOnlyList<TimeZoneInfo> All
);