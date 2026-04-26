using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cinturon360.Web.Services.State;

/// <summary>
/// Scoped Blazor circuit state: exposes the current user's preferences
/// (theme, language, time zone) and reactive notification helpers.
/// </summary>
public sealed class AppState
{
    private string _theme    = "system";
    private string _language = "en-AU";
    private string _timeZone = "Australia/Sydney";

    public string Theme    => _theme;
    public string Language => _language;
    public string TimeZone => _timeZone;

    public event Action? OnChange;

    /// <summary>Hydrate from the auth principal on circuit start.</summary>
    public void HydrateFromPrincipal(ClaimsPrincipal principal)
    {
        _theme    = principal.FindFirstValue(Security.ClaimTypes.Theme)    ?? "system";
        _language = principal.FindFirstValue(Security.ClaimTypes.Language) ?? "en-AU";
        _timeZone = principal.FindFirstValue(Security.ClaimTypes.TimeZone) ?? "Australia/Sydney";
        NotifyStateChanged();
    }

    public void SetTheme(string theme)
    {
        if (_theme == theme) return;
        _theme = theme;
        NotifyStateChanged();
    }

    public void SetLanguage(string language)
    {
        if (_language == language) return;
        _language = language;
        NotifyStateChanged();
    }

    public void SetTimeZone(string timeZone)
    {
        if (_timeZone == timeZone) return;
        _timeZone = timeZone;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
