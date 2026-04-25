using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Domain.Entities.Identity;

namespace Cinturon360.Application.Features.Users.Commands;

public sealed class CreateUserCommandHandler(
    IUserRepository userRepo,
    IUserSecurityRepository securityRepo,
    IPasswordHasher passwordHasher,
    IUnitOfWork uow)
    : IRequestHandler<CreateUserCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (await userRepo.ExistsByEmailAsync(request.Email, ct))
            return Result.Failure<string>(UserErrors.EmailAlreadyInUse);

        var user = User.Create(
            IdGenerator.NewUserId(),
            request.Email,
            request.FirstName,
            request.LastName,
            request.UserCategory,
            request.HomeOrgId,
            request.PlatformRole);

        await userRepo.AddAsync(user, ct);

        var security = UserSecurity.Create(
            IdGenerator.New("usec"),
            user.Id,
            allowDirectLogin: true);

        if (!string.IsNullOrWhiteSpace(request.PlaintextPassword))
            security.SetPasswordHash(passwordHasher.Hash(request.PlaintextPassword));

        await securityRepo.AddAsync(security, ct);
        await uow.SaveChangesAsync(ct);

        return Result.Success(user.Id);
    }
}
