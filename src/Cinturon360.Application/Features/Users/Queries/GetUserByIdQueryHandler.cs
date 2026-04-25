using MediatR;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Features.Users.Commands;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Users;

namespace Cinturon360.Application.Features.Users.Queries;

public sealed class GetUserByIdQueryHandler(IUserRepository userRepo)
    : IRequestHandler<GetUserByIdQuery, Result<UserDetail>>
{
    public async Task<Result<UserDetail>> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        var user = await userRepo.GetByIdAsync(request.UserId, ct);
        if (user is null)
            return Result.Failure<UserDetail>(UserErrors.NotFound);

        return Result.Success(new UserDetail(
            UserId: user.Id,
            Email: user.Email,
            FirstName: user.FirstName,
            LastName: user.LastName,
            FullName: user.FullName,
            UserCategory: user.UserCategory.ToString(),
            PlatformRole: user.PlatformRole?.ToString(),
            HomeOrgId: user.HomeOrgId,
            IsActive: user.IsActive,
            IsLocked: user.IsLocked,
            IsSuspended: user.IsSuspended,
            IsEmailVerified: user.IsEmailVerified,
            LanguageCode: user.LanguageCode,
            TimeZone: user.TimeZone,
            CurrencyCode: user.CurrencyCode,
            LastLoginAt: user.LastLoginAt,
            CreatedAt: user.CreatedAt));
    }
}
