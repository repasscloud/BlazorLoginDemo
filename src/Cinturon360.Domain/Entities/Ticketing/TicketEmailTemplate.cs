using Cinturon360.Domain.Common.Base;

namespace Cinturon360.Domain.Entities.Ticketing;

/// <summary>
/// DB-backed email template for support-ticket notifications.
/// One logical template code can have multiple language variants.
/// </summary>
public sealed class TicketEmailTemplate : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string LanguageCode { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string HtmlBody { get; private set; } = string.Empty;
    public string? PlainTextBody { get; private set; }
    public bool IsActive { get; private set; } = true;

    private TicketEmailTemplate() { }

    public static TicketEmailTemplate Create(
        string id,
        string code,
        string languageCode,
        string htmlBody,
        string? plainTextBody,
        string? description)
    {
        return new TicketEmailTemplate
        {
            Id = id,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            Code = code.Trim(),
            LanguageCode = languageCode.Trim(),
            HtmlBody = htmlBody,
            PlainTextBody = string.IsNullOrWhiteSpace(plainTextBody) ? null : plainTextBody,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            IsActive = true
        };
    }

    public void UpdateContent(string htmlBody, string? plainTextBody, string? description, bool isActive)
    {
        HtmlBody = htmlBody;
        PlainTextBody = string.IsNullOrWhiteSpace(plainTextBody) ? null : plainTextBody;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        IsActive = isActive;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}