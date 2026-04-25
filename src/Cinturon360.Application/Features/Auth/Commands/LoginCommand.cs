using MediatR;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Auth;

namespace Cinturon360.Application.Features.Auth.Commands;

/// <summary>
/// Authenticates a user with local email/password credentials.
/// Returns auth response on success, failure result on invalid credentials or locked account.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password,
    string? MfaCode,
    string? IpAddress,
    string? UserAgent,
    string? DeviceId = null
) : IRequest<Result<AuthResponse>>;
