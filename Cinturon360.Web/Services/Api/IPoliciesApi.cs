using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Web.Services.Api;

public interface IPoliciesApi
{
    Task<(TravelPolicy? Policy, ProblemDetails? Problem)> CreateTravelPolicyAsync(
        TravelPolicy policy,
        CancellationToken ct = default);
}
