namespace BlazorLoginDemo.Shared.Models.DTOs;

public class TravelPolicyBookingContextDto
{
    public string? Id { get; set; }
    public string? PolicyName { get; set; }
    public string? AvaClientId { get; set; }
    public string? Currency { get; set; }
    public decimal? MaxFlightPrice { get; set; }
    public string? DefaultFlightSeating { get; set; }
    public string? MaxFlightSeating { get; set; }
    public string? CabinClassCoverage { get; set; }
    public string? FlightBookingTimeAvailableFrom { get; set; }
    public string? FlightBookingTimeAvailableTo { get; set; }
    public List<string>? IncludedAirlineCodes { get; set; }
    public List<string>? ExcludedAirlineCodes { get; set; }
}
