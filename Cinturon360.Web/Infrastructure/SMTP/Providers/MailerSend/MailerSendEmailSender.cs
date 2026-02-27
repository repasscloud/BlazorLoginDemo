using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using Cinturon360.Web.Infrastructure.SMTP.Models;

namespace Cinturon360.Web.Infrastructure.SMTP.Providers.MailerSend;

public sealed class MailerSendEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly MailerSendEmailOptions _opts;

    public MailerSendEmailSender(
        IOptions<MailerSendEmailOptions> options,
        IHttpClientFactory factory)
    {
        _opts = options.Value;
        _http = factory.CreateClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _opts.ApiToken);
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
        SendInternalAsync(email, subject, htmlMessage, null, null);

    public Task SendWithAttachmentsAsync(
        string email,
        string subject,
        string html,
        IEnumerable<EmailAttachment>? attachments)
        => SendInternalAsync(email, subject, html, null, attachments);

    private async Task SendInternalAsync(
        string toEmail,
        string subject,
        string html,
        string? textOverride,
        IEnumerable<EmailAttachment>? attachments)
    {
        var payload = new
        {
            from = new { email = _opts.FromEmail, name = _opts.FromName },
            to = new[] { new { email = toEmail } },
            subject,
            html,
            text = textOverride ?? HtmlToText(html),
            attachments = attachments?.Select(a => new
            {
                filename = a.FileName,
                content = Convert.ToBase64String(a.Bytes)
            })
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PostAsync("https://api.mailersend.com/v1/email", content);

        if (!resp.IsSuccessStatusCode)
            throw new HttpRequestException(
                $"MailerSend {(int)resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
    }

    private static string HtmlToText(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var s = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
        s = Regex.Replace(s, @"</(p|div|h[1-6]|li)>", "\n", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "<.*?>", string.Empty);
        return System.Net.WebUtility.HtmlDecode(s).Trim();
    }
}
