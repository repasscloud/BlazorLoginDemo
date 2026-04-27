using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

/// <summary>Initiates a password reset flow. Always returns Success to prevent email enumeration.</summary>
public sealed record RequestPasswordResetCommand(string Email) : IRequest<Result>;
