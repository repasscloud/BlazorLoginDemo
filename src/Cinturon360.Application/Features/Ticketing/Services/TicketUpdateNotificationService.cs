using System.Text.RegularExpressions;
using Cinturon360.Application.Abstractions.Persistence;
using Cinturon360.Application.Abstractions.Services;
using Cinturon360.Common.Constants;
using Cinturon360.Domain.Entities.Identity;
using Cinturon360.Domain.Entities.Organization;
using Cinturon360.Domain.Entities.Ticketing;
using Microsoft.Extensions.Logging;

namespace Cinturon360.Application.Features.Ticketing.Services;

public sealed partial class TicketUpdateNotificationService : ITicketUpdateNotificationService
{
    private const string DefaultTemplateCode = "template_support_ticket_default";

    private readonly IUserRepository _users;
    private readonly IOrganisationRepository _organisations;
    private readonly ITicketEmailTemplateRepository _templates;
    private readonly IEmailService _email;
    private readonly ILogger<TicketUpdateNotificationService> _logger;

    public TicketUpdateNotificationService(
        IUserRepository users,
        IOrganisationRepository organisations,
        ITicketEmailTemplateRepository templates,
        IEmailService email,
        ILogger<TicketUpdateNotificationService> logger)
    {
        _users = users;
        _organisations = organisations;
        _templates = templates;
        _email = email;
        _logger = logger;
    }

    public async Task NotifyPublicUpdateAsync(
        SupportTicket ticket,
        string updateType,
        string actorUserId,
        string actorDisplayName,
        string body,
        CancellationToken ct = default)
    {
        if (!ticket.EmailMeUpdates)
            return;

        if (ticket.RaisedByUserId == actorUserId)
            return;

        var user = await _users.GetByIdAsync(ticket.RaisedByUserId, ct);
        if (!HasUsableEmail(user))
            return;

        var org = await _organisations.GetByIdNoTrackingAsync(ticket.OrgId, ct);
        var resolvedLanguage = NormalizeLanguage(user!.LanguageCode);
        var template = await ResolveTemplateAsync(org?.SupportTicketEmailTemplateCode, resolvedLanguage, ct);
        var effectiveLanguage = template?.LanguageCode ?? AppConstants.DefaultCulture;
        var variables = BuildVariables(ticket, user, org, updateType, actorDisplayName, body);
        var subject = BuildSubject(ticket, updateType, effectiveLanguage);

        var html = template is null
            ? BuildFallbackHtml(subject, variables)
            : Interpolate(template.HtmlBody, variables);

        var text = template?.PlainTextBody is null
            ? BuildFallbackText(subject, variables)
            : Interpolate(template.PlainTextBody, variables);

        await _email.SendAsync(user.Email, user.FullName, subject, html, text, ct);

        _logger.LogInformation(
            "EVT=TicketEmailNotification CAT=Ticketing ACT=Notify OUT=Sent TICKET={Ticket} TYPE={Type} TO={To} TEMPLATE={Template}",
            ticket.Id,
            updateType,
            user.Email,
            template?.Code ?? "fallback");
    }

    private async Task<TicketEmailTemplate?> ResolveTemplateAsync(string? orgTemplateCode, string languageCode, CancellationToken ct)
    {
        foreach (var code in CandidateCodes(orgTemplateCode))
        {
            var exact = await _templates.GetByCodeAndLanguageAsync(code, languageCode, ct);
            if (exact?.IsActive == true)
                return exact;

            var fallback = await _templates.GetByCodeAndLanguageAsync(code, AppConstants.DefaultCulture, ct);
            if (fallback?.IsActive == true)
                return fallback;
        }

        return null;
    }

    private static IEnumerable<string> CandidateCodes(string? orgTemplateCode)
    {
        if (!string.IsNullOrWhiteSpace(orgTemplateCode))
            yield return orgTemplateCode.Trim();

        yield return DefaultTemplateCode;
    }

    private static Dictionary<string, string> BuildVariables(
        SupportTicket ticket,
        User user,
        Organisation? org,
        string updateType,
        string actorDisplayName,
        string body)
    {
        return new(StringComparer.OrdinalIgnoreCase)
        {
            ["ticket_id"] = ticket.Id,
            ["ticket_subject"] = ticket.Subject,
            ["ticket_status"] = ticket.Status.ToString(),
            ["ticket_priority"] = ticket.Priority.ToString(),
            ["ticket_queue"] = ticket.Queue.ToString(),
            ["ticket_category"] = ticket.Category.ToString(),
            ["ticket_created_at_utc"] = ticket.CreatedAt.ToString("u"),
            ["ticket_updated_at_utc"] = ticket.UpdatedAt.ToString("u"),
            ["ticket_url"] = $"/support/{ticket.Id}",
            ["update_type"] = updateType,
            ["update_body"] = body,
            ["updated_by_name"] = actorDisplayName,
            ["updated_at_utc"] = DateTimeOffset.UtcNow.ToString("u"),
            ["user_full_name"] = user.FullName,
            ["user_email"] = user.Email,
            ["org_name"] = org?.Name ?? string.Empty,
            ["support_team_name"] = string.IsNullOrWhiteSpace(org?.SupportTeamName) ? "Support Team" : org.SupportTeamName!
        };
    }

    private static string BuildSubject(SupportTicket ticket, string updateType, string languageCode)
    {
        return languageCode switch
        {
            "fr-FR" => $"Mise a jour du ticket {ticket.Id}: {ticket.Subject}",
            _ => $"Support ticket update {ticket.Id}: {ticket.Subject}"
        };
    }

    private static string BuildFallbackHtml(string subject, IReadOnlyDictionary<string, string> variables)
    {
        return $"<html><body><h2>{Escape(subject)}</h2><p>Hello {Escape(variables["user_full_name"])},</p><p>Your support ticket has a public update.</p><p><strong>Update type:</strong> {Escape(variables["update_type"])}</p><p><strong>Updated by:</strong> {Escape(variables["updated_by_name"])}</p><p><strong>Message:</strong><br />{Escape(variables["update_body"]).Replace("\n", "<br />")}</p><p><a href=\"{Escape(variables["ticket_url"])}\">View ticket</a></p></body></html>";
    }

    private static string BuildFallbackText(string subject, IReadOnlyDictionary<string, string> variables)
    {
        return $"{subject}\n\nHello {variables["user_full_name"]},\n\nYour support ticket has a public update.\n\nUpdate type: {variables["update_type"]}\nUpdated by: {variables["updated_by_name"]}\n\n{variables["update_body"]}\n\nView ticket: {variables["ticket_url"]}";
    }

    private static string Interpolate(string template, IReadOnlyDictionary<string, string> variables)
        => PlaceholderRegex().Replace(template, match =>
        {
            var key = match.Groups[1].Value;
            return variables.TryGetValue(key, out var value) ? value : string.Empty;
        });

    private static string NormalizeLanguage(string? languageCode)
        => languageCode switch
        {
            null or "" => AppConstants.DefaultCulture,
            "en" => AppConstants.DefaultCulture,
            "fr" => "fr-FR",
            _ => languageCode
        };

    private static bool HasUsableEmail(User? user)
        => user is not null && !string.IsNullOrWhiteSpace(user.Email) && user.Email.Contains('@');

    private static string Escape(string value)
        => System.Net.WebUtility.HtmlEncode(value);

    [GeneratedRegex("\\{\\{\\s*([a-zA-Z0-9_]+)\\s*\\}\\}")]
    private static partial Regex PlaceholderRegex();
}