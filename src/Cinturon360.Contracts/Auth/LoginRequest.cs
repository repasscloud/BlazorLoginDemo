namespace Cinturon360.Contracts.Auth;

/// <summary>Request body for local username/password login.</summary>
public sealed record LoginRequest(
    string Email,
    string Password,
    string? MfaCode = null,
    string? DeviceId = null
);
