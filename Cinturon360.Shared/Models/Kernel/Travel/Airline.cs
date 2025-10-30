using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Shared.Models.Kernel.Travel;

public sealed class Airline
{
    public int Id { get; set; }
    public string Iata { get; set; } = default!;  // e.g., QF
    public string Icao { get; set; } = string.Empty;  // optional
    public string Name { get; set; } = default!;
    public string Alias { get; set; } = string.Empty;  // optional
    public string CallSign { get; set; } = string.Empty;  // optional
    public string Country { get; set; } = default!;
    public AirlineAlliance Alliance { get; set; } = AirlineAlliance.None;
    public int? FoundedYear { get; set; }

    // 0..1 back-link
    public LoyaltyProgram? LoyaltyProgram { get; set; }
}