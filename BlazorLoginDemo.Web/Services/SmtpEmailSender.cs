using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using Microsoft.AspNetCore.Identity;

namespace BlazorLoginDemo.Web.Services;

// Implements both generic and non-generic for compatibility
public sealed class SmtpEmailSender :
    IEmailSender<ApplicationUser>,
    IEmailSender
{
    private readonly SmtpOptions _opts;

    public SmtpEmailSender(IOptions<SmtpOptions> options)
        => _opts = options.Value;

    // Non-generic sender
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
        => SendAsyncInternal(email, subject, htmlMessage);

    // Generic sender methods
    Task IEmailSender<ApplicationUser>.SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        => SendAsyncInternal(
            email,
            "Confirm your email",
            $"<p>Please confirm your account by <a href=\"{confirmationLink}\">clicking here</a>.</p>");

    Task IEmailSender<ApplicationUser>.SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        => SendAsyncInternal(
            email,
            "Reset your password",
            $"<p>Reset your password by <a href=\"{resetLink}\">clicking here</a>.</p>");

    Task IEmailSender<ApplicationUser>.SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        => SendAsyncInternal(
            email,
            "Password reset code",
            $"<p>Your password reset code is: <strong>{resetCode}</strong></p>");

    private async Task SendAsyncInternal(string toEmail, string subject, string html)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opts.FromName, _opts.FromEmail));
        msg.To.Add(MailboxAddress.Parse(toEmail));
        msg.Subject = subject;

        var body = new BodyBuilder { HtmlBody = html };
        msg.Body = body.ToMessageBody();

        using var client = new SmtpClient();
        var secure = _opts.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect;
        await client.ConnectAsync(_opts.Host, _opts.Port, secure);
        if (!string.IsNullOrEmpty(_opts.User))
            await client.AuthenticateAsync(_opts.User, _opts.Password);

        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }
}
