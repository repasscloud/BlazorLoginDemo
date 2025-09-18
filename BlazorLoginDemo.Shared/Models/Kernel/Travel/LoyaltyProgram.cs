
using BlazorLoginDemo.Shared.Models.Static.Travel;

namespace BlazorLoginDemo.Shared.Models.Kernel.Travel;

public sealed class LoyaltyProgram
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;     // "QFF", "AAdvantage", "KrisFlyer"
    public string Name { get; set; } = default!;
    public int AirlineId { get; set; }
    public Airline Airline { get; set; } = default!;

    public AirlineAlliance Alliance { get; set; }           // mirror Airline.Alliance for easy filtering
    public bool IsActive { get; set; } = true;
}