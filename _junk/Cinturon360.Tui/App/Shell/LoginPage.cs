using Cinturon360.Tui.App.Services.Auth;
using Cinturon360.Tui.App.Theming;
using Cinturon360.Tui.App.UI.Dialogs;
using Terminal.Gui;

namespace Cinturon360.Tui.App.Shell;

/// <summary>
/// Modal login UI. Returns true when login succeeds.
/// </summary>
public sealed class LoginPage
{
    private readonly IAuthService _authService;
    private readonly IMessageBoxService _msgBox;
    private readonly IBusyDialogService _busy;

    public LoginPage(IAuthService authService,
                     IMessageBoxService msgBox,
                     IBusyDialogService busy)
    {
        _authService = authService;
        _msgBox = msgBox;
        _busy = busy;
    }

    public bool Show()
    {
        bool success = false;

        var dialog = new Dialog("Cinturon360 Login", 60, 14)
        {
            ColorScheme = AppTheme.DialogScheme
        };

        var lblUser = new Label("Username:")
        {
            X = 2,
            Y = 2
        };

        var txtUser = new TextField("")
        {
            X = Pos.Right(lblUser) + 1,
            Y = Pos.Top(lblUser),
            Width = 30
        };

        var lblPass = new Label("Password:")
        {
            X = 2,
            Y = Pos.Bottom(lblUser) + 1
        };

        var txtPass = new TextField("")
        {
            X = Pos.Right(lblPass) + 1,
            Y = Pos.Top(lblPass),
            Width = 30,
            Secret = true
        };

        var btnLogin = new Button("Login")
        {
            IsDefault = true,
            X = Pos.Center() - 10,
            Y = Pos.Bottom(lblPass) + 2
        };

        var btnQuit = new Button("Quit")
        {
            X = Pos.Center() + 2,
            Y = Pos.Top(btnLogin)
        };

        btnLogin.Clicked += async () =>
        {
            var username = txtUser.Text.ToString() ?? string.Empty;
            var password = txtPass.Text.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                _msgBox.Error("Login failed", "Username and password are required.");
                return;
            }

            try
            {
                await _busy.RunWithBusyOverlayAsync(
                    "Authenticating...",
                    async ct =>
                    {
                        var ok = await _authService.AuthenticateAsync(username, password, ct);
                        success = ok;
                    });

                if (!success)
                {
                    _msgBox.Error("Login failed", "Invalid credentials.");
                    return;
                }

                Application.RequestStop(dialog);
            }
            catch (OperationCanceledException)
            {
                _msgBox.Info("Login cancelled", "Authentication cancelled.");
            }
            catch (Exception ex)
            {
                _msgBox.Error("Login error", ex.Message);
            }
        };

        btnQuit.Clicked += () =>
        {
            success = false;
            Application.RequestStop(dialog);
        };

        dialog.Add(lblUser, txtUser, lblPass, txtPass, btnLogin, btnQuit);

        Application.Run(dialog);

        return success;
    }
}
