using Cinturon360.Tui.App.Infrastructure.Http;
using Cinturon360.Tui.App.Services.Auth;
using Cinturon360.Tui.App.Shell;
using Cinturon360.Tui.App.UI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;   // <<< add this

namespace Cinturon360.Tui.App;

public static class AppBootstrapper
{
    public static TuiApp Build(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            // <<< STOP console logging so it doesn't trash the TUI
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();                 // remove Console, Debug, etc.
                logging.SetMinimumLevel(LogLevel.Warning); // keep the pipeline, but nothing chatty
            })
            .ConfigureServices((ctx, services) =>
            {
                // HTTP factory + named clients
                services.AddHttpClient();

                services.AddSingleton<IApiClientFactory, ApiClientFactory>();

                // Auth service – later point to your Cinturon360 backend
                services.AddSingleton<IAuthService, AuthService>();

                // UI helpers
                services.AddSingleton<IMessageBoxService, MessageBoxService>();
                services.AddSingleton<IBusyDialogService, BusyDialogService>();

                // Shell
                services.AddSingleton<MainShell>();
            })
            .Build();

        return new TuiApp(host);
    }
}
