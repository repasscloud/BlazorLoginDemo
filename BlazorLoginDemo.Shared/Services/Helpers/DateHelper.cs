using System.Globalization;

namespace BlazorLoginDemo.Shared.Services.Helpers;

public static class DateHelper
{
    private const string YMD = "yyyy-MM-dd";

    // string? / string -> "YYYY-MM-DD" or null
    public static string? ToYmd(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        if (DateTimeOffset.TryParse(
                input,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dto))
        {
            return DateOnly.FromDateTime(dto.UtcDateTime)
                           .ToString(YMD, CultureInfo.InvariantCulture);
        }

        if (DateTime.TryParse(
                input,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            return DateOnly.FromDateTime(dt)
                           .ToString(YMD, CultureInfo.InvariantCulture);
        }

        return null;
    }

    // DateTime? / DateTime -> "YYYY-MM-DD" or null
    public static string? ToYmd(DateTime? input)
    {
        if (input is null) return null;

        var dt = input.Value;
        if (dt.Kind == DateTimeKind.Local) dt = dt.ToUniversalTime();
        return DateOnly.FromDateTime(dt)
                       .ToString(YMD, CultureInfo.InvariantCulture);
    }
}
