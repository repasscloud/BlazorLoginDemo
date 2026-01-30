using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Helpers;
using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Shared.Models.Kernel.Travel;

public sealed class Airline
{
    [Key]
    [MaxLength(12)]
    public string Id { get; set; } = IDGeneratorHelper.GenerateId(IdGenType.Airline);
    public string Iata { get; set; } = default!;  // e.g., QF
    public string? Icao { get; set; }
    public string Name { get; set; } = default!;
    public string Alias { get; set; } = string.Empty;  // optional
    public string CallSign { get; set; } = string.Empty;  // optional
    public string Country { get; set; } = default!;
    public AirlineAlliance Alliance { get; set; } = AirlineAlliance.None;
    public int? FoundedYear { get; set; }

    // 0..1 back-link
    public LoyaltyProgram? LoyaltyProgram { get; set; }
}