using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Common.IdGeneration;
using Cinturon360.Common.Results;
using Cinturon360.Contracts.Auth;
using Cinturon360.Domain.Entities.Identity;
using Cinturon360.Domain.Enums.Security;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepo,
    IUserSecurityRepository securityRepo,
    IUserSessionRepository sessionRepo,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork uow,
    ILogger<RegisterUserCommandHandler> logger)
    : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);
    private const int MinPasswordLength = 8;

    public async Task<Result<AuthResponse>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        if (request.Password.Length < MinPasswordLength)
            return Result.Failure<AuthResponse>(AuthErrors.WeakPassword);

        if (await userRepo.ExistsByEmailAsync(request.Email, ct))
            return Result.Failure<AuthResponse>(AuthErrors.EmailAlreadyRegistered);

        var user = User.Create(
            id: IdGenerator.NewUserId(),
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            category: UserCategory.Client,
            homeOrgId: null);

        await userRepo.AddAsync(user, ct);

        var security = UserSecurity.Create(IdGenerator.New("usec"), user.Id);
        security.SetPasswordHash(passwordHasher.Hash(request.Password));
        await securityRepo.AddAsync(security, ct);

        // Issue a session immediately so the user is logged in after registration
        var jti = Guid.NewGuid().ToString();
        var expiresAt = DateTimeOffset.UtcNow.Add(AccessTokenLifetime);
        var permissions = new List<string>();

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.HomeOrgId, jti, permissions);

        var session = UserSession.Create(
            IdGenerator.NewSessionId(),
            user.Id,
            TokenClass.InteractiveWebSession,
            jti,
            expiresAt,
            deviceId: null,
            request.UserAgent,
            request.IpAddress);

        await sessionRepo.AddAsync(session, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} registered from {Ip}", user.Id, request.IpAddress);

        return Result.Success(new AuthResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresIn: (int)AccessTokenLifetime.TotalSeconds,
            SessionId: session.Id,
            User: new UserInfoResponse(
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                OrgId: null,
                UserCategory: user.UserCategory.ToString(),
                PlatformRole: null,
                Permissions: permissions)));
    }
}
