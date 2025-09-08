using BlazorLoginDemo.Shared.Data;
using BlazorLoginDemo.Shared.Services.Interfaces.External;
using Microsoft.AspNetCore.Mvc;

namespace BlazorLoginDemo.Api.Controllers.Flights;

[Microsoft.AspNetCore.Components.Route("api/vi/flights")]
[ApiController]
public class FlightsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IAmadeusAuthService _authService;

    public FlightsController(ApplicationDbContext db, IAmadeusAuthService authService)
    {
        _db = db;
        _authService = authService;
    }
}