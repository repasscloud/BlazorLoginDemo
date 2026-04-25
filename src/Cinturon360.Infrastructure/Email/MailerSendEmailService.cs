using Cinturon360.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Cinturon360.Infrastructure.Email;

/// <summary>
/// Stub MailerSend email service implementation.
/// TODO: Replace with real MailerSend HTTP client when API key is configured.
/// </summary>
internal sealed class MailerSendEmailService : IEmailService
{
    private readonly ILogger<MailerSendEmailService> _logger;

    public MailerSendEmailService(ILogger<MailerSendEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[EmailStub] Send to {To} ({Name}) — Subject: {Subject}",
            toEmail, toName, subject);
        return Task.CompletedTask;
    }

    public Task SendTemplatedAsync(
        string toEmail,
        string toName,
        string templateId,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "[EmailStub] SendTemplated to {To} ({Name}) — TemplateId: {TemplateId} Variables: {Count}",
            toEmail, toName, templateId, variables.Count);
        return Task.CompletedTask;
    }
}
