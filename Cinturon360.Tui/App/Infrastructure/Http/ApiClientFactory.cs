using System.Net.Http;
using System.Net.Http.Headers;

namespace Cinturon360.Tui.App.Infrastructure.Http;

/// <summary>
/// Centralised HttpClient creation. Extend as needed.
/// </summary>
public sealed class ApiClientFactory : IApiClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public HttpClient CreateClient(string name)
    {
        var client = _httpClientFactory.CreateClient();

        switch (name)
        {
            case ApiClientName.CinturonApi:
                ConfigureCinturon(client);
                break;

            case ApiClientName.Github:
                ConfigureGithub(client);
                break;

            case ApiClientName.ServiceNow:
                ConfigureServiceNow(client);
                break;

            default:
                // Generic client – caller sets BaseAddress, headers etc.
                break;
        }

        return client;
    }

    public HttpClient CreateCinturonClient() => CreateClient(ApiClientName.CinturonApi);

    public HttpClient CreateGithubClient() => CreateClient(ApiClientName.Github);

    public HttpClient CreateServiceNowClient() => CreateClient(ApiClientName.ServiceNow);

    private static void ConfigureCinturon(HttpClient client)
    {
        client.BaseAddress = new Uri("https://api.cinturon360.example/"); // TODO
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private static void ConfigureGithub(HttpClient client)
    {
        client.BaseAddress = new Uri("https://api.github.com/");
        client.Timeout = TimeSpan.FromSeconds(30);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        client.DefaultRequestHeaders.UserAgent.ParseAdd("Cinturon360.Tui");
    }

    private static void ConfigureServiceNow(HttpClient client)
    {
        client.BaseAddress = new Uri("https://cinturon360.service-now.com/api/now/"); // TODO
        client.Timeout = TimeSpan.FromSeconds(60);
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }
}
