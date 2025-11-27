using Cinturon360.Shared.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinturon360.Api.Controllers.Admin;

[Route("api/v1/admin/ui")]
[ApiController]
public class AdminUIController : ControllerBase
{
    private readonly ApplicationDbContext _db;

    public AdminUIController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpPost("sync-policies")]
    public async Task<bool> SyncPoliciesAsync()
    {
        // load users TRACKED (we’re going to modify them)
        var users = await _db.AvaUserSysPreferences
            .AsTracking()
            .ToListAsync();

        // load only the policy fields you actually need (FAST: projects less data)
        //    Replace the sample fields with the ones you need in your app.
        var policies = await _db.TravelPolicies
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                // 1:1 name matches
                p.PolicyName,
                p.DefaultFlightSeating,
                p.MaxFlightSeating,
                p.IncludedAirlineCodes,
                p.ExcludedAirlineCodes,
                p.CabinClassCoverage,
                p.NonStopFlight,
                p.DefaultCurrencyCode,
                p.FlightBookingTimeAvailableFrom,
                p.FlightBookingTimeAvailableTo,
                p.EnableSaturdayFlightBookings,
                p.EnableSundayFlightBookings,
                p.DefaultCalendarDaysInAdvanceForFlightBooking,
            })
            .ToDictionaryAsync(x => x.Id.ToString());

        int updated = 0;
        foreach (var u in users)
        {
            if (u.TravelPolicyId == null) continue;
            if (!policies.TryGetValue(u.TravelPolicyId, out var p)) continue;

            u.TravelPolicyName = p.PolicyName;
            u.DefaultFlightSeating = p.DefaultFlightSeating;
            u.MaxFlightSeating = p.MaxFlightSeating;
            u.IncludedAirlineCodes = p.IncludedAirlineCodes.Length > 0 ? u.IncludedAirlineCodes : p.IncludedAirlineCodes;
            u.ExcludedAirlineCodes = p.ExcludedAirlineCodes.Length > 0 ? u.ExcludedAirlineCodes : p.ExcludedAirlineCodes;
            u.CabinClassCoverage = p.CabinClassCoverage;
            u.NonStopFlight = p.NonStopFlight ?? u.NonStopFlight;
            u.DefaultCurrencyCode = string.IsNullOrWhiteSpace(p.DefaultCurrencyCode) ? u.DefaultCurrencyCode : p.DefaultCurrencyCode;
            u.FlightBookingTimeAvailableFrom = string.IsNullOrWhiteSpace(p.FlightBookingTimeAvailableFrom) ? u.FlightBookingTimeAvailableFrom : p.FlightBookingTimeAvailableFrom;
            u.FlightBookingTimeAvailableTo = string.IsNullOrWhiteSpace(p.FlightBookingTimeAvailableTo) ? u.FlightBookingTimeAvailableTo : p.FlightBookingTimeAvailableTo;
            u.EnableSaturdayFlightBookings = p.EnableSaturdayFlightBookings ?? u.EnableSaturdayFlightBookings;
            u.EnableSundayFlightBookings = p.EnableSundayFlightBookings ?? u.EnableSundayFlightBookings;
            u.DefaultCalendarDaysInAdvanceForFlightBooking = p.DefaultCalendarDaysInAdvanceForFlightBooking ?? u.DefaultCalendarDaysInAdvanceForFlightBooking;

            updated++;
        }

        if (updated > 0)
            await _db.SaveChangesAsync();

        return true;
    }
}
