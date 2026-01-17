namespace Cinturon360.Shared.Models.ExternalLib.Amadeus;

public sealed class AmadeusConnectionTestResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public string? Details { get; init; }
}
