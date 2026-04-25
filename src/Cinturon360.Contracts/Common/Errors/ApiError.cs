namespace Cinturon360.Contracts.Common.Errors;

/// <summary>Standardised error body for API error responses.</summary>
public sealed record ApiError(
    string Code,
    string Message,
    IReadOnlyList<string>? Details = null
);
