using Cinturon360.Tui.App;

namespace Cinturon360.Tui;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var app = AppBootstrapper.Build(args);
        await app.RunAsync();
    }
}
