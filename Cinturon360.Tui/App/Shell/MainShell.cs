using Cinturon360.Tui.App.Infrastructure.Http;
using Cinturon360.Tui.App.Services.Auth;
using Cinturon360.Tui.App.Theming;
using Cinturon360.Tui.App.UI.Dialogs;
using Cinturon360.Tui.App.Pages.Org;          // <- bring ChildrenOrgPage into scope
using Terminal.Gui;
using Cinturon360.Tui.App.Pages.Pnr;

namespace Cinturon360.Tui.App.Shell;

/// <summary>
/// Top-level Terminal.Gui shell.
/// </summary>
public sealed class MainShell : IPageNavigator
{
    private readonly IAuthService _authService;
    private readonly IMessageBoxService _msgBox;
    private readonly IBusyDialogService _busy;
    private readonly IApiClientFactory _apiClientFactory;

    private Toplevel _top = null!;
    private Window _mainWindow = null!;
    private ListView _menuList = null!;
    private FrameView _detailHost = null!;
    private StatusBar _statusBar = null!;

    private readonly List<(string Name, Func<IPage> Factory)> _pages = new();
    private IPage? _currentPage;

    public MainShell(
        IAuthService authService,
        IMessageBoxService msgBox,
        IBusyDialogService busy,
        IApiClientFactory apiClientFactory)
    {
        _authService = authService;
        _msgBox = msgBox;
        _busy = busy;
        _apiClientFactory = apiClientFactory;

        // register initial pages
        _pages.Add(("Dashboard", () => new DummyDashboardPage()));
        _pages.Add(("Travel Admin", () => new DummyAdminPage()));

        // Children Org page – explicitly return IPage to match Func<IPage>
        // _pages.Add(("Children Org", () => (IPage)new ChildrenOrgPage(_apiClientFactory)));
        _pages.Add(("Children Org", () => (IPage)new ChildrenOrgPage(_apiClientFactory, this)));

        // PNR
        _pages.Add(("New PNR", () => new NewPnrPage()));

        // add more pages here later
    }

    public void Run()
    {
        Application.Init();
        try
        {
            _top = Application.Top;

            // Always set theme early
            AppTheme.SetDarkTheme();

            // Show login first
            var loginPage = new LoginPage(_authService, _msgBox, _busy);
            var loggedIn = loginPage.Show();
            if (!loggedIn)
                return;

            BuildChrome();
            Application.Run(_top);
        }
        finally
        {
            Application.Shutdown();
        }
    }

    private void BuildChrome()
    {
        // Menu bar (top)
        var menu = new MenuBar(new[]
        {
            new MenuBarItem("_File", new[]
            {
                new MenuItem("_Logout", "", Logout),
                new MenuItem("_Quit", "", () => Application.RequestStop())
            }),
            new MenuBarItem("_Help", new[]
            {
                new MenuItem("_Help",  "", ShowHelp),   // <<< new dedicated help
                new MenuItem("_About", "", ShowAbout)  // <<< keep About separate
            })
        })
        {
            ColorScheme = AppTheme.MenuScheme
        };

        // Main content window
        _mainWindow = new Window("Cinturon360")
        {
            X = 0,
            Y = 1, // below menu
            Width = Dim.Fill(),
            Height = Dim.Fill() - 1, // above status bar
            ColorScheme = AppTheme.WindowScheme
        };

        BuildBodyLayout();

        // Status bar
        _statusBar = new StatusBar(new[]
        {
            new StatusItem(Key.F1, "~F1~ Help", ShowHelp),             // <<< F1 => Help
            new StatusItem(Key.Q | Key.CtrlMask, "~Ctrl-Q~ Quit", () => Application.RequestStop())
        })
        {
            ColorScheme = AppTheme.StatusScheme
        };

        _top.Add(menu, _mainWindow, _statusBar);
    }

    private void BuildBodyLayout()
    {
        // Left menu frame
        var navFrame = new FrameView("Menu")
        {
            X = 0,
            Y = 0,
            Width = 25,
            Height = Dim.Fill(),
            ColorScheme = AppTheme.WindowScheme
        };

        _menuList = new ListView(_pages.Select(p => p.Name).ToList())
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        _menuList.OpenSelectedItem += args =>
        {
            ActivatePage(args.Item);
        };

        _menuList.KeyPress += e =>
        {
            if (e.KeyEvent.Key == Key.Enter)
            {
                ActivatePage(_menuList.SelectedItem);
                e.Handled = true;
            }
        };

        navFrame.Add(_menuList);

        // Right detail frame
        _detailHost = new FrameView("Detail")
        {
            X = Pos.Right(navFrame),
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = AppTheme.WindowScheme
        };

        _mainWindow.Add(navFrame, _detailHost);
    }

    // private void ActivatePage(int index)
    // {
    //     if (index < 0 || index >= _pages.Count)
    //         return;

    //     _currentPage?.OnDeactivated();
    //     _detailHost.RemoveAll();

    //     var (name, factory) = _pages[index];
    //     var page = factory();
    //     _currentPage = page;

    //     // ESC on any page returns focus to menu
    //     page.View.KeyPress += args =>
    //     {
    //         if (args.KeyEvent.Key == Key.Esc)
    //         {
    //             _menuList.SetFocus();
    //             args.Handled = true;
    //         }
    //     };

    //     page.OnActivated();

    //     _detailHost.Title = name;
    //     page.View.X = 0;
    //     page.View.Y = 0;
    //     page.View.Width = Dim.Fill();
    //     page.View.Height = Dim.Fill();

    //     _detailHost.Add(page.View);
    //     page.View.SetFocus();
    // }
    private void ActivatePage(int index)
    {
        if (index < 0 || index >= _pages.Count)
            return;

        var (name, factory) = _pages[index];
        var page = factory();

        ShowPage(page, name);
    }

    // new method
    public void ShowPage(IPage page, string title)
    {
        _currentPage?.OnDeactivated();
        _detailHost.RemoveAll();

        _currentPage = page;

        // ESC on any page returns focus to menu
        page.View.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Esc)
            {
                _menuList.SetFocus();
                args.Handled = true;
            }
        };

        page.OnActivated();

        _detailHost.Title = title;
        page.View.X = 0;
        page.View.Y = 0;
        page.View.Width = Dim.Fill();
        page.View.Height = Dim.Fill();

        _detailHost.Add(page.View);
        page.View.SetFocus();
    }


    private void Logout()
    {
        // You can clear auth info here
        _msgBox.Info("Logout", "You have been logged out.");
        Application.RequestStop(); // simple behaviour for now
        // Later you can loop back into Login if you want.
    }

    private void ShowAbout()
    {
        _msgBox.Info(
            "About Cinturon360.Tui",
            "Cinturon360 Terminal Client\nVersion 0.1.0\n© RePass Cloud Pty Ltd 2025");
    }

    private void ShowHelp()
    {
        const int width  = 78;
        const int height = 20;

        var dialog = new Dialog("Help", width, height)
        {
            ColorScheme = AppTheme.DialogScheme
        };

        var helpText = string.Join(Environment.NewLine, new[]
        {
            "Cinturon360.Tui – Help",
            "",
            "Navigation:",
            "  - Arrow Up / Down : Move through the left-hand Menu list.",
            "  - Enter           : Open the selected page in the Detail panel.",
            "  - Esc             : Return focus to the Menu from a page.",
            "",
            "Global keys:",
            "  - F1              : Show this help.",
            "  - Ctrl+Q          : Quit the application.",
            "",
            "Layout:",
            "  - Top    : Menu bar (File, Help).",
            "  - Left   : Menu – list of functional areas (Dashboard, Travel Admin, ...).",
            "  - Right  : Detail – content of the currently selected page.",
            "",
            "Login:",
            "  - For now, any non-empty username and password will log in.",
            "  - This is a placeholder; later it will call the Cinturon360 auth API.",
            "",
            "Extending:",
            "  - Add new pages by implementing IPage / PageBase and",
            "    registering them in the MainShell constructor.",
            "",
            "Press Enter or Esc to close this help."
        });

        var textView = new TextView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(2),
            Height = Dim.Fill(2),
            ReadOnly = true,
            WordWrap = true,
            Text = helpText
        };

        var closeButton = new Button("OK")
        {
            IsDefault = true,
            X = Pos.Center(),
            Y = Pos.Bottom(textView)
        };
        closeButton.Clicked += () => Application.RequestStop(dialog);

        dialog.Add(textView, closeButton);

        dialog.KeyPress += args =>
        {
            if (args.KeyEvent.Key == Key.Esc)
            {
                Application.RequestStop(dialog);
                args.Handled = true;
            }
        };

        Application.Run(dialog);
    }

    /// <summary>
    /// Temporary stub pages until you build real ones.
    /// </summary>
    private sealed class DummyDashboardPage : PageBase
    {
        public override string Title => "Dashboard";

        public DummyDashboardPage()
        {
            var lbl = new Label("Dashboard will show key travel metrics here.")
            {
                X = 1,
                Y = 1
            };

            _root.Add(lbl);
        }
    }

    private sealed class DummyAdminPage : PageBase
    {
        public override string Title => "Travel Admin";

        public DummyAdminPage()
        {
            var lbl = new Label("Travel admin tools will live here.")
            {
                X = 1,
                Y = 1
            };

            _root.Add(lbl);
        }
    }
}
