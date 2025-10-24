
using BlazorLoginDemo.Shared.Models.Static.Travel;

namespace BlazorLoginDemo.Shared.Models.Kernel.Travel;

public sealed class LoyaltyProgram
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;  // "QFF", "AAdvantage", "KrisFlyer"
    public string Name { get; set; } = default!;
    public int AirlineId { get; set; }  // required FK to Airline
    public Airline Airline { get; set; } = default!;

    public AirlineAlliance Alliance { get; set; } = AirlineAlliance.Unknown;  // mirror Airline.Alliance
    public bool IsActive { get; set; } = true;
}