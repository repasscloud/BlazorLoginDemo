namespace Cinturon360.Tui.App.Services.Auth;

/// <summary>
/// Stub auth. Replace with calls to Cinturon360 auth API.
/// </summary>
public sealed class AuthService : IAuthService
{
    public string? CurrentUser { get; private set; }

    public async Task<bool> AuthenticateAsync(string username, string password, CancellationToken ct = default)
    {
        // fake a tiny bit of "work"
        await Task.Delay(250, ct);

        var ok = !string.IsNullOrWhiteSpace(username) &&
                !string.IsNullOrWhiteSpace(password);

        CurrentUser = ok ? username : null;
        return ok;
    }
}
