namespace Cinturon360.Web.Infrastructure.SMTP.Providers.Smtp;

public sealed class SmtpEmailOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string User { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromEmail { get; init; } = string.Empty;
    public string FromName { get; init; } = "Cinturon360";
    public bool UseStartTls { get; init; } = true;
}
