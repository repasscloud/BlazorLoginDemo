using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

namespace BlazorLoginDemo.Web.Services;

public sealed class MailerSendEmailSender :
    IEmailSender<ApplicationUser>,
    IEmailSender
{
    private readonly HttpClient _http;
    private readonly MailerSendOptions _opts;

    public MailerSendEmailSender(IOptions<MailerSendOptions> options, IHttpClientFactory httpClientFactory)
    {
        _opts = options.Value;
        _http = httpClientFactory.CreateClient();
        _http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _opts.ApiToken);
    }

    // Non-generic IEmailSender
    public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
        SendAsyncInternal(email, subject, htmlMessage, textOverride: null, attachments: null);

    // Generic IEmailSender<ApplicationUser>
    Task IEmailSender<ApplicationUser>.SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendAsyncInternal(
            email,
            "Confirm your email",
            $"<p>Please confirm your account by <a href=\"{confirmationLink}\">clicking here</a>.</p>",
            textOverride: $"Please confirm your account: {confirmationLink}",
            attachments: null);

    Task IEmailSender<ApplicationUser>.SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendAsyncInternal(
            email,
            "Reset your password",
            $"<p>Reset your password by <a href=\"{resetLink}\">clicking here</a>.</p>",
            textOverride: $"Reset your password: {resetLink}",
            attachments: null);

    Task IEmailSender<ApplicationUser>.SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        SendAsyncInternal(
            email,
            "Password reset code",
            $"<p>Your password reset code is: <strong>{resetCode}</strong></p>",
            textOverride: $"Your password reset code is: {resetCode}",
            attachments: null);

    // Convenience API with attachments support
    public Task SendEmailAsync(
        string toEmail,
        string subject,
        string html,
        string? textFallback,
        IEnumerable<EmailAttachment>? attachments)
        => SendAsyncInternal(toEmail, subject, html, textFallback, attachments);

    private async Task SendAsyncInternal(
        string toEmail,
        string subject,
        string html,
        string? textOverride,
        IEnumerable<EmailAttachment>? attachments)
    {
        var text = textOverride ?? HtmlToText(html);

        var payload = new
        {
            from = new { email = _opts.FromEmail, name = _opts.FromName },
            to = new[] { new { email = toEmail } },
            subject,
            html,
            text,
            attachments = attachments?.Select(a => new
            {
                filename = a.FileName,
                content = Convert.ToBase64String(a.Bytes)
            })
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var resp = await _http.PostAsync("https://api.mailersend.com/v1/email", content);
        
        // trow with body for easier debugging if MailerSend is unhappy
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync();
            throw new HttpRequestException($"MailerSend {((int)resp.StatusCode)}: {resp.ReasonPhrase}\n{body}");
        }
    }

    private static string HtmlToText(string html)
    {
        if (string.IsNullOrWhiteSpace(html)) return string.Empty;
        var s = Regex.Replace(html, @"<(br|BR)\s*/?>", "\n");
        s = Regex.Replace(s, @"</(p|div|h[1-6]|li)>", "\n", RegexOptions.IgnoreCase);
        s = Regex.Replace(s, "<.*?>", string.Empty);
        s = System.Net.WebUtility.HtmlDecode(s);
        s = Regex.Replace(s, @"[ \t]+\n", "\n").Trim();
        return s;
    }
}
