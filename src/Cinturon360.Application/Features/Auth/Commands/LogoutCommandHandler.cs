using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed class LogoutCommandHandler(
    IUserSessionRepository sessionRepo,
    IUnitOfWork uow)
    : IRequestHandler<LogoutCommand, Result>
{
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var session = await sessionRepo.GetByJtiAsync(request.SessionId, ct);
        if (session is null)
            return Result.Failure(AuthErrors.SessionNotFound);

        if (session.UserId != request.UserId)
            return Result.Failure(AuthErrors.Unauthorized);

        if (session.IsRevoked)
            return Result.Success();

        session.Revoke("user_logout");
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
