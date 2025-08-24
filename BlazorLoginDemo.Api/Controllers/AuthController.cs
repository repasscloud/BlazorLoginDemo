using BlazorLoginDemo.Shared.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.User;
using BlazorLoginDemo.Api.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AvaUser> _userManager;
    private readonly SignInManager<AvaUser> _signInManager;
    private readonly TokenService _tokenService;

    public AuthController(UserManager<AvaUser> userManager,
                          SignInManager<AvaUser> signInManager,
                          TokenService tokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
    }

    public record LoginRequest([Required] string UsernameOrEmail, [Required] string Password);
    public record TokenResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresUtc);

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var user = await _userManager.FindByNameAsync(req.UsernameOrEmail)
                   ?? await _userManager.FindByEmailAsync(req.UsernameOrEmail);

        if (user is null) return Unauthorized();

        var result = await _signInManager.CheckPasswordSignInAsync(user, req.Password, lockoutOnFailure: true);
        if (!result.Succeeded) return Unauthorized();

        var (access, refresh) = await _tokenService.GenerateTokensAsync(user, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return new TokenResponse(access, refresh.Token, refresh.ExpiresUtc);
    }

    public record RefreshRequest([Required] string RefreshToken);

    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var refreshed = await _tokenService.RefreshAsync(req.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (refreshed is null) return Unauthorized();

        var (access, refresh) = refreshed.Value;
        return new TokenResponse(access, refresh.Token, refresh.ExpiresUtc);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] RefreshRequest req, CancellationToken ct)
    {
        var ok = await _tokenService.RevokeAsync(req.RefreshToken, HttpContext.Connection.RemoteIpAddress?.ToString(), "Manual revoke", ct);
        return ok ? NoContent() : NotFound();
    }
}
