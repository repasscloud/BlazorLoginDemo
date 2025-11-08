using Cinturon360.Shared.Services.Interfaces.External;
using Microsoft.AspNetCore.Mvc;

namespace Cinturon360.Api.Controllers.Admin;

[ApiController]
[Route("api/v1/admin/ingress")]
public sealed class IngressController : ControllerBase
{
    private readonly IAirlineService _airlineService;

    public IngressController(IAirlineService airlineService) => _airlineService = airlineService;

    // ------ Airlines ------
    [HttpGet("airlines-data")]
    public async Task<IActionResult> IngressIATADataAsync(CancellationToken ct)
    {
        var result = await _airlineService.ImportFromConfiguredSourceAsync(ct);
        return Ok(result);
    }
}
