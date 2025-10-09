using System.Net.Http;
using System.Threading;
using System;
using System.Threading.Tasks;
class Program
{
    static async Task Main(string[] args)
    {
        var url = "http://127.0.0.1:8080/healthz";
        var timeout = TimeSpan.FromSeconds(2);
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--url" && i + 1 < args.Length) url = args[++i];
            else if (args[i] == "--timeout" && i + 1 < args.Length) timeout = TimeSpan.Parse(args[++i]);
        }
        using var cts = new CancellationTokenSource(timeout);
        using var http = new HttpClient();
        try
        {
            using var resp = await http.GetAsync(url, cts.Token);
            Environment.Exit(resp.IsSuccessStatusCode ? 0 : 1);
        }
        catch { Environment.Exit(1); }
    }
}

