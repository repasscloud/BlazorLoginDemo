using Cinturon360.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Cinturon360.Infrastructure.Email;

/// <summary>
/// MailerSend email service implementation.
/// Falls back to no-op logging if MailerSend is not configured.
/// </summary>
internal sealed class MailerSendEmailService : IEmailService
{
    private static readonly Uri DefaultBaseUri = new("https://api.mailersend.com");

    private readonly HttpClient _httpClient;
    private readonly MailerSendSettings _settings;
    private readonly ILogger<MailerSendEmailService> _logger;

    public MailerSendEmailService(
        HttpClient httpClient,
        IOptions<MailerSendSettings> settings,
        ILogger<MailerSendEmailService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_settings.BaseUrl)
            && Uri.TryCreate(_settings.BaseUrl, UriKind.Absolute, out var parsed))
        {
            _httpClient.BaseAddress = parsed;
        }
        else
        {
            _httpClient.BaseAddress ??= DefaultBaseUri;
        }

        if (!string.IsNullOrWhiteSpace(_settings.ApiToken)
            && !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiToken);
        }
    }

    public async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string? plainTextBody = null,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning(
                "MailerSend not configured. Email delivery skipped for {To}.",
                toEmail);
            return;
        }

        var payload = new MailerSendEmailRequest(
            new MailerSendAddress(_settings.FromEmail, _settings.FromName),
            [new MailerSendAddress(toEmail, toName)],
            subject,
            htmlBody,
            plainTextBody,
            null,
            null);

        await PostEmailAsync(payload, toEmail, ct);
    }

    public async Task SendTemplatedAsync(
        string toEmail,
        string toName,
        string templateId,
        IReadOnlyDictionary<string, string> variables,
        CancellationToken ct = default)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning(
                "MailerSend not configured. Templated email delivery skipped for {To} (Template {TemplateId}).",
                toEmail,
                templateId);
            return;
        }

        var payload = new MailerSendEmailRequest(
            new MailerSendAddress(_settings.FromEmail, _settings.FromName),
            [new MailerSendAddress(toEmail, toName)],
            null,
            null,
            null,
            templateId,
            [new MailerSendPersonalization(toEmail, new Dictionary<string, string>(variables))]);

        await PostEmailAsync(payload, toEmail, ct);
    }

    private bool IsConfigured()
        => !string.IsNullOrWhiteSpace(_settings.ApiToken)
           && !string.IsNullOrWhiteSpace(_settings.FromEmail);

    private async Task PostEmailAsync(MailerSendEmailRequest payload, string toEmail, CancellationToken ct)
    {
        try
        {
            using var response = await _httpClient.PostAsJsonAsync("/v1/email", payload, ct);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("MailerSend email delivered to {To}", toEmail);
                return;
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError(
                "MailerSend delivery failed for {To}. Status={StatusCode} Body={Body}",
                toEmail,
                (int)response.StatusCode,
                body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MailerSend request failed for {To}", toEmail);
        }
    }
}

public sealed class MailerSendSettings
{
    public string ApiToken { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Cinturon360";
    public string BaseUrl { get; set; } = "https://api.mailersend.com";
}

internal sealed record MailerSendAddress(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("name")] string Name);

internal sealed record MailerSendPersonalization(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("data")] IReadOnlyDictionary<string, string> Data);

internal sealed record MailerSendEmailRequest(
    [property: JsonPropertyName("from")] MailerSendAddress From,
    [property: JsonPropertyName("to")] IReadOnlyList<MailerSendAddress> To,
    [property: JsonPropertyName("subject")] string? Subject,
    [property: JsonPropertyName("html")] string? Html,
    [property: JsonPropertyName("text")] string? Text,
    [property: JsonPropertyName("template_id")] string? TemplateId,
    [property: JsonPropertyName("personalization")] IReadOnlyList<MailerSendPersonalization>? Personalization);
