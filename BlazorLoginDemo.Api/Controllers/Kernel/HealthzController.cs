using Microsoft.AspNetCore.Mvc;

namespace BlazorLoginDemo.Api.Controllers.Kernel;

[Route("api/v1/healthz")]
[ApiController]
public class HealthzController : ControllerBase
{
    [HttpGet("check")]
    public IActionResult HealthCheck() => Ok(new { ok = true, now = DateTime.UtcNow });
}