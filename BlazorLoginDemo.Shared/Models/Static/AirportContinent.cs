using System.Runtime.Serialization;

namespace BlazorLoginDemo.Shared.Models.Static;

public enum AirportContinent
{
    [EnumMember(Value = "")]
    Unknown = 0,

    [EnumMember(Value = "AF")]
    AF = 1,

    [EnumMember(Value = "AN")]
    AN = 2,

    [EnumMember(Value = "AS")]
    AS = 3,

    [EnumMember(Value = "EU")]
    EU = 4,

    [EnumMember(Value = "NA")]
    NA = 5,

    [EnumMember(Value = "OC")]
    OC = 6,

    [EnumMember(Value = "SA")]
    SA = 7
}
