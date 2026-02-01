using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Cinturon360.Web.Infrastructure.SMTP.Providers.Smtp;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly SmtpEmailOptions _opts;

    public SmtpEmailSender(IOptions<SmtpEmailOptions> options)
        => _opts = options.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_opts.FromName, _opts.FromEmail));
        msg.To.Add(MailboxAddress.Parse(email));
        msg.Subject = subject;

        msg.Body = new BodyBuilder { HtmlBody = htmlMessage }.ToMessageBody();

        using var client = new SmtpClient();
        var secure = _opts.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.SslOnConnect;

        await client.ConnectAsync(_opts.Host, _opts.Port, secure);
        if (!string.IsNullOrEmpty(_opts.User))
            await client.AuthenticateAsync(_opts.User, _opts.Password);

        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }
}
