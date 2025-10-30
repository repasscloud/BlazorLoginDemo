using Microsoft.AspNetCore.Mvc;
using Cinturon360.Shared.Security;

namespace Cinturon360.Api.Controllers.Test;

[Route("api/v1/test")]
[ServiceFilter(typeof(RequireApiKeyFilter))]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, now = DateTime.UtcNow });
}