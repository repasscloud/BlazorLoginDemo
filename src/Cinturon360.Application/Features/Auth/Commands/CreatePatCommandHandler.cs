using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Auth;
using Cinturon360.Domain.Entities.Identity;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed class CreatePatCommandHandler(
    IUserApiTokenRepository tokenRepo,
    ITokenService tokenService,
    IUnitOfWork uow)
    : IRequestHandler<CreatePatCommand, Result<CreatePatResponse>>
{
    public async Task<Result<CreatePatResponse>> Handle(CreatePatCommand request, CancellationToken ct)
    {
        var (rawToken, prefix, hash) = tokenService.GeneratePat();

        var token = UserApiToken.Create(
            IdGenerator.NewApiTokenId(),
            request.UserId,
            TokenClass.UserPat,
            request.Name,
            hash,
            prefix,
            request.ExpiresAt,
            request.Scopes);

        await tokenRepo.AddAsync(token, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(new CreatePatResponse(
            TokenId: token.Id,
            Name: token.Name,
            RawToken: rawToken,
            TokenPrefix: prefix,
            ExpiresAt: token.ExpiresAt));
    }
}
