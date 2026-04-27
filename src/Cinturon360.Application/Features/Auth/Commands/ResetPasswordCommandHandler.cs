using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Security;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepo,
    IUserSecurityRepository securityRepo,
    IPasswordHasher passwordHasher,
    IUnitOfWork uow,
    ILogger<ResetPasswordCommandHandler> logger)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    private const int MinPasswordLength = 8;

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        if (request.NewPassword.Length < MinPasswordLength)
            return Result.Failure(AuthErrors.WeakPassword);

        var user = await userRepo.GetByEmailAsync(request.Email, ct);
        if (user is null || !user.IsActive)
            return Result.Failure(AuthErrors.InvalidResetToken);

        var security = await securityRepo.GetByUserIdAsync(user.Id, ct);
        if (security is null)
            return Result.Failure(AuthErrors.InvalidResetToken);

        // Hash the incoming raw token to compare against stored hash
        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));

        if (!security.IsPasswordResetTokenValid(tokenHash))
        {
            logger.LogWarning("Invalid or expired password reset token for user {UserId}", user.Id);
            return Result.Failure(AuthErrors.InvalidResetToken);
        }

        // Apply new password and clear the token
        var newHash = passwordHasher.Hash(request.NewPassword);
        security.SetPasswordHash(newHash);
        security.ClearPasswordResetToken();
        security.ClearFailedLogins();

        await uow.SaveChangesAsync(ct);

        logger.LogInformation("Password reset completed for user {UserId}", user.Id);
        return Result.Success();
    }
}
