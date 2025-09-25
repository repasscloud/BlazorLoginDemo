using BlazorLoginDemo.Shared.Services.Interfaces.External;
using Microsoft.AspNetCore.Mvc;

namespace BlazorLoginDemo.Api.Controllers.Amadeus;

[Route("api/amadeus/auth")]
[ApiController]
public class AmadeusAuthController : ControllerBase
{
    private readonly IAmadeusAuthService _authService;

    public AmadeusAuthController(IAmadeusAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("get-token")]
    public async Task<IActionResult> GetToken()
    {
        var token = await _authService.GetTokenInformationAsync();
        return Ok(token);
    }
}
