using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.Extensions.Logging;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Common.Results;

namespace Cinturon360.Application.Features.Auth.Commands;

public sealed class RequestPasswordResetCommandHandler(
    IUserRepository userRepo,
    IUserSecurityRepository securityRepo,
    IEmailService emailService,
    IUnitOfWork uow,
    ILogger<RequestPasswordResetCommandHandler> logger)
    : IRequestHandler<RequestPasswordResetCommand, Result>
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(1);

    public async Task<Result> Handle(RequestPasswordResetCommand request, CancellationToken ct)
    {
        // Always return success — never reveal whether the email exists.
        var user = await userRepo.GetByEmailAsync(request.Email, ct);
        if (user is null || !user.IsActive)
        {
            logger.LogInformation("Password reset requested for unknown or inactive email {Email}", request.Email);
            return Result.Success();
        }

        var security = await securityRepo.GetByUserIdAsync(user.Id, ct);
        if (security is null)
            return Result.Success();

        // Generate a cryptographically secure random token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var rawToken = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-").Replace("/", "_").TrimEnd('='); // URL-safe

        var tokenHash = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));

        var expiresAt = DateTimeOffset.UtcNow.Add(TokenLifetime);

        security.SetPasswordResetToken(tokenHash, expiresAt);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Password reset token issued for user {UserId} (expires {ExpiresAt})",
            user.Id, expiresAt);

        // Send reset email (stub logs only — real email wired in Phase 10)
        var resetPath = $"/auth/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(rawToken)}";
        await emailService.SendAsync(
            toEmail: user.Email,
            toName: user.FullName,
            subject: "Reset your Cinturon360 password",
            htmlBody: $"<p>Hi {user.FirstName},</p><p>Use the link below to reset your password (expires in 1 hour).</p><p>Path: {resetPath}</p><p>If you did not request this, ignore this email.</p>",
            ct: ct);

        return Result.Success();
    }
}
