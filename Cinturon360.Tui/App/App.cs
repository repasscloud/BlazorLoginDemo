using Cinturon360.Tui.App.Shell;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Cinturon360.Tui.App;

public sealed class TuiApp
{
    private readonly IHost _host;

    public TuiApp(IHost host)
    {
        _host = host;
    }

    public async Task RunAsync()
    {
        using (_host)
        {
            await _host.StartAsync();

            var shell = _host.Services.GetRequiredService<MainShell>();
            shell.Run(); // blocking until user quits

            await _host.StopAsync();
        }
    }
}
