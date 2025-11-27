using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Web.Services.Api;

public sealed class PoliciesApi : IPoliciesApi
{
    private readonly IHttpClientFactory _factory;

    public PoliciesApi(IHttpClientFactory factory) => _factory = factory;

    public async Task<(TravelPolicy? Policy, ProblemDetails? Problem)> CreateTravelPolicyAsync(
        TravelPolicy policy,
        CancellationToken ct = default)
    {
        var http = _factory.CreateClient("AvaApi");

        using var resp = await http.PostAsJsonAsync("/v1/policies/travel", policy, ct);

        if (resp.StatusCode == HttpStatusCode.Created)
        {
            var created = await resp.Content.ReadFromJsonAsync<TravelPolicy>(cancellationToken: ct);
            return (created, null);
        }

        var pd = await resp.Content.ReadFromJsonAsync<ProblemDetails>(cancellationToken: ct)
                 ?? new ProblemDetails { Status = (int)resp.StatusCode, Title = "Request failed" };

        return (null, pd);
    }
}
