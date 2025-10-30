using System.Runtime.Serialization;

namespace Cinturon360.Shared.Models.Static;

public enum AirportType
{
    Unknown = 0,
    
    [EnumMember(Value = "SmallAirport")]
    SmallAirport = 20,

    [EnumMember(Value = "MediumAirport")]
    MediumAirport = 21,

    [EnumMember(Value = "LargeAirport")]
    LargeAirport = 22,

    [EnumMember(Value = "Heliport")]
    Heliport = 30,

    [EnumMember(Value = "SeaplaneBase")]
    SeaplanePort = 31,

    [EnumMember(Value = "Balloonport")]
    BalloonPort = 32,

    [EnumMember(Value = "Closed")]
    Closed = 99
}
