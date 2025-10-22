using System.Globalization;

namespace BlazorLoginDemo.Shared.Services.Helpers;

public static class DateHelper
{
    /// <summary>
    /// Parses many ISO-8601 datetime strings (e.g., "...Z", "...+00:00") and returns "YYYY-MM-DD".
    /// Returns null if input is null/empty or unparsable.
    /// </summary>
    public static string? TODateOnlyString(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        if (DateTimeOffset.TryParse(
            input,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
            out var dto))
        {
            return DateOnly.FromDateTime(dto.UtcDateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        // Fallback for odd cases without offset
        if (DateTime.TryParse(
                input,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            return DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return null;
    }
}

public static class DateStringExtensions
{
    /// <summary>
    /// Convert many ISO-8601 datetime strings (e.g. "2025-10-22T12:21:39.7502214Z", "2025-10-26T00:00:00+00:00")
    /// to "yyyy-MM-dd". Returns null if input is null/empty/unparsable.
    /// </summary>
    public static string? ToDateOnlyString(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        if (DateTimeOffset.TryParse(
                input,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dto))
        {
            return DateOnly.FromDateTime(dto.UtcDateTime).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        if (DateTime.TryParse(
                input,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var dt))
        {
            return DateOnly.FromDateTime(dt).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        return null;
    }
}

public static class DateExtensions
{
    // DateTime? -> DateOnly?
    public static DateOnly? ToDateOnly(this DateTime? input)
    {
        if (input is null) return null;
        return DateOnly.FromDateTime(input.Value);
    }

    // DateTime -> DateOnly
    public static DateOnly ToDateOnly(this DateTime input) =>
        DateOnly.FromDateTime(input);

    // Optional: handle DateTimeOffset inputs
    public static DateOnly? ToDateOnly(this DateTimeOffset? input)
    {
        if (input is null) return null;
        return DateOnly.FromDateTime(input.Value.UtcDateTime);
    }

    public static DateOnly ToDateOnly(this DateTimeOffset input) =>
        DateOnly.FromDateTime(input.UtcDateTime);
}