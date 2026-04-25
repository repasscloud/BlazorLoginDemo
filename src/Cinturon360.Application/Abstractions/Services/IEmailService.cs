namespace Cinturon360.Application.Abstractions.Services;

/// <summary>
/// Abstraction for transactional email delivery.
/// Implemented by MailerSend in Infrastructure.Integrations.
/// </summary>
public interface IEmailService
{
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default);

    Task SendTemplatedAsync(
        string toEmail,
        string toName,
        string templateId,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken ct = default);
}
