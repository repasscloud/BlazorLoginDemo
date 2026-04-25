using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed class RevokePatCommandHandler(
    IUserApiTokenRepository tokenRepo,
    IUnitOfWork uow)
    : IRequestHandler<RevokePatCommand, Result>
{
    public async Task<Result> Handle(RevokePatCommand request, CancellationToken ct)
    {
        var tokens = await tokenRepo.GetActiveByUserIdAsync(request.RequestingUserId, ct);
        var token = tokens.FirstOrDefault(t => t.Id == request.TokenId);

        if (token is null)
            return Result.Failure(AuthErrors.TokenNotFound);

        token.Revoke();
        await uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
