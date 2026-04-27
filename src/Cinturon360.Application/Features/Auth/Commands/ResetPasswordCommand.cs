using MediatR;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword) : IRequest<Result>;
