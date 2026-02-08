using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Helpers;
using Cinturon360.Shared.Models.Static.Travel;
using Cinturon360.Shared.Validation;

namespace Cinturon360.Shared.Models.Kernel.Travel;

public sealed class Airline
{
    [Key]
    [MaxLength(12)]
    public string Id { get; set; } = IDGeneratorHelper.GenerateId(IdGenType.Airline);
    public string? DuffelId { get; set; }  // optional, for integration with Duffel API
    [Required, AlphaNumeric2Validation] public string Iata { get; set; } = default!;  // e.g., QF
    public string? Icao { get; set; }
    public string Name { get; set; } = default!;
    public string? Alias { get; set; }  // optional
    public string? CallSign { get; set; }  // optional
    public string? Country { get; set; }  // optional
    public AirlineAlliance Alliance { get; set; } = AirlineAlliance.None;
    public int? FoundedYear { get; set; }  // optional

    public string? LogoSymbolUrl { get; set; }  // optional
    public string? LogoLockupUrl { get; set; }  // optional
    public string? ConditionsOfCarriageUrl { get; set; }  // optional

    // 0..1 back-link
    public LoyaltyProgram? LoyaltyProgram { get; set; }
}
