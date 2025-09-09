using System.Runtime.Serialization;

namespace BlazorLoginDemo.Shared.Models.Static;

public enum AirportType
{
    Unknown = 0,
    
    [EnumMember(Value = "small_airport")]
    SmallAirport = 20,

    [EnumMember(Value = "medium_airport")]
    MediumAirport = 21,

    [EnumMember(Value = "large_airport")]
    LargeAiport = 22,

    [EnumMember(Value = "heliport")]
    Heliport = 30,

    [EnumMember(Value = "seaplane_base")]
    SeaplanePort = 31,

    [EnumMember(Value = "balloonport")]
    BalloonPort = 32,

    [EnumMember(Value = "closed")]
    Closed = 99
}
