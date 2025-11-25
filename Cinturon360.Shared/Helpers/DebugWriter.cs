using System.Text;
using System.Text.Json;

namespace Cinturon360.Shared.Helpers;

public static class DebugWriter
{
    // Keep opts here if you want a consistent debug shape across the app:
    // - PropertyNamingPolicy typically set by your APIs; leaving null preserves C# property names.
    // - WriteIndented = true makes diffs readable.
    public static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    /// <summary>
    /// Writes any payload to disk as JSON for debugging.
    /// Does not require the caller to specify the type explicitly.
    /// </summary>
    public static async Task WriteJsonDebugAsync<T>(
        T payload,
        string filePrefix = "debug",
        string? debugDir = null,
        JsonSerializerOptions? jsonOptions = null,
        CancellationToken ct = default)
    {
        // Serialize generically; no runtime type knowledge needed by the caller.
        // (If payload is null and T is reference type, serialize will output "null".)
        string jsonString = JsonSerializer.Serialize(payload, jsonOptions ?? JsonOpts);

        // Default debug dir under the app base directory (works for services/console/apps).
        debugDir ??= Path.Combine(AppContext.BaseDirectory, "debug-data");
        Directory.CreateDirectory(debugDir);

        // Use a timestamp to avoid collisions.
        string filePath = Path.Combine(
            debugDir,
            $"{filePrefix}-{DateTime.UtcNow:yyyyMMdd_HHmmss_fff}.json");

        await File.WriteAllTextAsync(filePath, jsonString, Encoding.UTF8, ct);

        // Returning the path is handy for logs/tests.
        return;
    }
}
