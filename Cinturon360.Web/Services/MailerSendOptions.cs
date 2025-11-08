namespace Cinturon360.Web.Services;

public sealed class MailerSendOptions
{
    public string ApiToken { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Cinturon360";
}
