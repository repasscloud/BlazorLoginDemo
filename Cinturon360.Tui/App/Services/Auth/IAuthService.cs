namespace Cinturon360.Tui.App.Services.Auth;

public interface IAuthService
{
    Task<bool> AuthenticateAsync(string username, string password, CancellationToken ct = default);

    string? CurrentUser { get; }
}
