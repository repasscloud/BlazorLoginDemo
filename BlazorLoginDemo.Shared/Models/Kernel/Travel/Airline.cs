using BlazorLoginDemo.Shared.Models.Static.Travel;

namespace BlazorLoginDemo.Shared.Models.Kernel.Travel;

public sealed class Airline
{
    public int Id { get; set; }
    public string Iata { get; set; } = default!;  // eg QF
    public string Icao { get; set; } = string.Empty;  // optional
    public string Name { get; set; } = default!;
    public AirlineAlliance Alliance { get; set; } = AirlineAlliance.Unknown;  // independant for non-alliance carrier
    public int? FoundedYear { get; set; }

    public ICollection<LoyaltyProgram> Programs { get; set; } = new List<LoyaltyProgram>();
}