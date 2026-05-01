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

public sealed class LoginCommandHandler(
    IUserRepository userRepo,
    IUserSecurityRepository securityRepo,
    IUserSessionRepository sessionRepo,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IUnitOfWork uow,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromMinutes(30);

    public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await userRepo.GetByEmailAsync(request.Email, ct);

        if (user is null || !user.IsActive || user.IsLocked || user.IsSuspended)
        {
            logger.LogWarning("Login failed for {Email} — user not found or inactive", request.Email);
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        var security = await securityRepo.GetByUserIdAsync(user.Id, ct);
        if (security is null || string.IsNullOrEmpty(security.PasswordHash))
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);

        // Check lockout
        if (security.LockoutUntil.HasValue && security.LockoutUntil > DateTimeOffset.UtcNow)
            return Result.Failure<AuthResponse>(AuthErrors.AccountLocked);

        if (!passwordHasher.Verify(request.Password, security.PasswordHash))
        {
            security.RecordFailedLogin();

            if (security.FailedLoginAttempts >= MaxFailedAttempts)
            {
                security.SetLockout(DateTimeOffset.UtcNow.Add(LockoutDuration));
                user.Lock();
                logger.LogWarning("User {UserId} locked after {Attempts} failed attempts", user.Id, security.FailedLoginAttempts);
            }

            await uow.SaveChangesAsync(ct);
            return Result.Failure<AuthResponse>(AuthErrors.InvalidCredentials);
        }

        // Successful credential check
        security.ClearFailedLogins();
        user.RecordLogin();

        var jti = Guid.NewGuid().ToString();
        var expiresAt = DateTimeOffset.UtcNow.Add(AccessTokenLifetime);

        // TODO: resolve permissions from role assignments
        var permissions = new List<string>();

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.HomeOrgId, jti, permissions);

        var session = UserSession.Create(
            IdGenerator.NewSessionId(),
            user.Id,
            TokenClass.InteractiveWebSession,
            jti,
            expiresAt,
            request.DeviceId,
            request.UserAgent,
            request.IpAddress);

        await sessionRepo.AddAsync(session, ct);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} logged in from {Ip}", user.Id, request.IpAddress);

        var response = new AuthResponse(
            AccessToken: accessToken,
            TokenType: "Bearer",
            ExpiresIn: (int)AccessTokenLifetime.TotalSeconds,
            SessionId: session.Id,
            User: new UserInfoResponse(
                UserId: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                OrgId: user.HomeOrgId,
                UserCategory: user.UserCategory.ToString(),
                PlatformRole: user.PlatformRole?.ToString(),
                Permissions: permissions));

        return Result.Success(response);
    }
}
