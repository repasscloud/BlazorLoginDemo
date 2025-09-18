using Microsoft.AspNetCore.Mvc;
using BlazorLoginDemo.Shared.Security;

namespace BlazorLoginDemo.Api.Controllers.Test;

[Route("api/v1/test")]
[ServiceFilter(typeof(RequireApiKeyFilter))]
[ApiController]
public class TestController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { ok = true, now = DateTime.UtcNow });
}