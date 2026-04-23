namespace Cinturon360.Web.Infrastructure.SMTP.Providers.MailerSend;

public sealed class MailerSendEmailOptions
{
    public string ApiToken { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = "Cinturon360";
}
