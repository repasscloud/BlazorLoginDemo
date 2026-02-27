using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Helpers;
using Cinturon360.Shared.Models.Static.Travel;

namespace Cinturon360.Shared.Models.Kernel.Travel;

public sealed class LoyaltyProgram
{
    [Key]
    [MaxLength(20)]
    public string Id { get; set; } = IDGeneratorHelper.GenerateId(IdGenType.LoyaltyAccount);
    public string Code { get; set; } = default!;  // "QFF", "AAdvantage", "KrisFlyer"
    public string Name { get; set; } = default!;
    public int AirlineId { get; set; }  // required FK to Airline
    public Airline Airline { get; set; } = default!;

    public AirlineAlliance Alliance { get; set; } = AirlineAlliance.None;  // mirror Airline.Alliance
    public bool IsActive { get; set; } = true;
}