using System.Net.Http;

namespace Cinturon360.Tui.App.Infrastructure.Http;

/// <summary>
/// Simple wrapper over HttpClient with logical names.
/// </summary>
public interface IApiClientFactory
{
    HttpClient CreateClient(string name);

    HttpClient CreateCinturonClient();

    HttpClient CreateGithubClient();

    HttpClient CreateServiceNowClient();
}
