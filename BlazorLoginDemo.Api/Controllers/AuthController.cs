using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using BlazorLoginDemo.Api.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _db;
    private readonly TokenService _tokens;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ApplicationDbContext db,
        TokenService tokens)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
        _tokens = tokens;
    }

    public record LoginRequest([Required] string UsernameOrEmail, [Required] string Password);
    public record TokenResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresUtc);

    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var appUser = await _userManager.FindByNameAsync(req.UsernameOrEmail)
                     ?? await _userManager.FindByEmailAsync(req.UsernameOrEmail);
        if (appUser is null) return Unauthorized();

        var pwd = await _signInManager.CheckPasswordSignInAsync(appUser, req.Password, lockoutOnFailure: true);
        if (!pwd.Succeeded) return Unauthorized();

        // Find the AvaUser profile by FK to AspNetUsersId
        var avaUser = await _db.AvaUsers.FirstOrDefaultAsync(u => u.AspNetUsersId == appUser.Id, ct);
        if (avaUser is null) return Problem("AvaUser profile missing for this account.");

        var (access, refresh) = await _tokens.GenerateTokensAsync(avaUser, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        return new TokenResponse(access, refresh.Token, refresh.ExpiresUtc);
    }
}
