using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Static.Travel;

/// <summary>
/// High-level types of travel components, supporting combinations via bit flags.
/// </summary>
[Flags]
public enum TravelComponentType
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Flight")]
    Flight = 1 << 1,

    [Display(Name = "Hotel")]
    Hotel = 1 << 2,

    [Display(Name = "Flight + Hotel")]
    Flight_Hotel = 1 << 3,

    [Display(Name = "Car")]
    Car = 1 << 4,

    [Display(Name = "Flight + Car")]
    Flight_Car = 1 << 5,

    [Display(Name = "Hotel + Car")]
    Hotel_Car = 1 << 6,

    [Display(Name = "Flight + Hotel + Car")]
    Flight_Hotel_Car = 1 << 7,

    [Display(Name = "Rail")]
    Rail = 1 << 8,

    [Display(Name = "Flight + Rail")]
    Flight_Rail = 1 << 9,

    [Display(Name = "Hotel + Rail")]
    Hotel_Rail = 1 << 10,

    [Display(Name = "Flight + Hotel + Rail")]
    Flight_Hotel_Rail = 1 << 11,

    [Display(Name = "Car + Rail")]
    Car_Rail = 1 << 12,

    [Display(Name = "Flight + Car + Rail")]
    Flight_Car_Rail = 1 << 13,

    [Display(Name = "Hotel + Car + Rail")]
    Hotel_Car_Rail = 1 << 14,

    [Display(Name = "Flight + Hotel + Car + Rail")]
    Flight_Hotel_Car_Rail = 1 << 15,

    [Display(Name = "Transfer")]
    Transfer = 1 << 16,

    [Display(Name = "Flight + Transfer")]
    Flight_Transfer = 1 << 17,

    [Display(Name = "Hotel + Transfer")]
    Hotel_Transfer = 1 << 18,

    [Display(Name = "Flight + Hotel + Transfer")]
    Flight_Hotel_Transfer = 1 << 19,

    [Display(Name = "Car + Transfer")]
    Car_Transfer = 1 << 20,

    [Display(Name = "Flight + Car + Transfer")]
    Flight_Car_Transfer = 1 << 21,

    [Display(Name = "Hotel + Car + Transfer")]
    Hotel_Car_Transfer = 1 << 22,

    [Display(Name = "Flight + Hotel + Car + Transfer")]
    Flight_Hotel_Car_Transfer = 1 << 23,

    [Display(Name = "Rail + Transfer")]
    Rail_Transfer = 1 << 24,

    [Display(Name = "Flight + Rail + Transfer")]
    Flight_Rail_Transfer = 1 << 25,

    [Display(Name = "Hotel + Rail + Transfer")]
    Hotel_Rail_Transfer = 1 << 26,

    [Display(Name = "Flight + Hotel + Rail + Transfer")]
    Flight_Hotel_Rail_Transfer = 1 << 27,

    [Display(Name = "Car + Rail + Transfer")]
    Car_Rail_Transfer = 1 << 28,

    [Display(Name = "Flight + Car + Rail + Transfer")]
    Flight_Car_Rail_Transfer = 1 << 29,

    [Display(Name = "Hotel + Car + Rail + Transfer")]
    Hotel_Car_Rail_Transfer = 1 << 30,

    [Display(Name = "Flight + Hotel + Car + Rail + Transfer")]
    Flight_Hotel_Car_Rail_Transfer = 1 << 31,

    [Display(Name = "Activity")]
    Activity = 1 << 32,

    [Display(Name = "Flight + Activity")]
    Flight_Activity = 1 << 33,

    [Display(Name = "Hotel + Activity")]
    Hotel_Activity = 1 << 34,

    [Display(Name = "Flight + Hotel + Activity")]
    Flight_Hotel_Activity = 1 << 35,

    [Display(Name = "Car + Activity")]
    Car_Activity = 1 << 36,

    [Display(Name = "Flight + Car + Activity")]
    Flight_Car_Activity = 1 << 37,

    [Display(Name = "Hotel + Car + Activity")]
    Hotel_Car_Activity = 1 << 38,

    [Display(Name = "Flight + Hotel + Car + Activity")]
    Flight_Hotel_Car_Activity = 1 << 39,

    [Display(Name = "Rail + Activity")]
    Rail_Activity = 1 << 40,

    [Display(Name = "Flight + Rail + Activity")]
    Flight_Rail_Activity = 1 << 41,

    [Display(Name = "Hotel + Rail + Activity")]
    Hotel_Rail_Activity = 1 << 42,

    [Display(Name = "Flight + Hotel + Rail + Activity")]
    Flight_Hotel_Rail_Activity = 1 << 43,

    [Display(Name = "Car + Rail + Activity")]
    Car_Rail_Activity = 1 << 44,

    [Display(Name = "Flight + Car + Rail + Activity")]
    Flight_Car_Rail_Activity = 1 << 45,

    [Display(Name = "Hotel + Car + Rail + Activity")]
    Hotel_Car_Rail_Activity = 1 << 46,

    [Display(Name = "Flight + Hotel + Car + Rail + Activity")]
    Flight_Hotel_Car_Rail_Activity = 1 << 47,

    [Display(Name = "Transfer + Activity")]
    Transfer_Activity = 1 << 48,

    [Display(Name = "Flight + Transfer + Activity")]
    Flight_Transfer_Activity = 1 << 49,

    [Display(Name = "Hotel + Transfer + Activity")]
    Hotel_Transfer_Activity = 1 << 50,

    [Display(Name = "Flight + Hotel + Transfer + Activity")]
    Flight_Hotel_Transfer_Activity = 1 << 51,

    [Display(Name = "Car + Transfer + Activity")]
    Car_Transfer_Activity = 1 << 52,

    [Display(Name = "Flight + Car + Transfer + Activity")]
    Flight_Car_Transfer_Activity = 1 << 53,

    [Display(Name = "Hotel + Car + Transfer + Activity")]
    Hotel_Car_Transfer_Activity = 1 << 54,

    [Display(Name = "Flight + Hotel + Car + Transfer + Activity")]
    Flight_Hotel_Car_Transfer_Activity = 1 << 55,

    [Display(Name = "Rail + Transfer + Activity")]
    Rail_Transfer_Activity = 1 << 56,

    [Display(Name = "Flight + Rail + Transfer + Activity")]
    Flight_Rail_Transfer_Activity = 1 << 57,

    [Display(Name = "Hotel + Rail + Transfer + Activity")]
    Hotel_Rail_Transfer_Activity = 1 << 58,

    [Display(Name = "Flight + Hotel + Rail + Transfer + Activity")]
    Flight_Hotel_Rail_Transfer_Activity = 1 << 59,

    [Display(Name = "Car + Rail + Transfer + Activity")]
    Car_Rail_Transfer_Activity = 1 << 60,

    [Display(Name = "Flight + Car + Rail + Transfer + Activity")]
    Flight_Car_Rail_Transfer_Activity = 1 << 61,

    [Display(Name = "Hotel + Car + Rail + Transfer + Activity")]
    Hotel_Car_Rail_Transfer_Activity = 1 << 62,

    [Display(Name = "Flight + Hotel + Car + Rail + Transfer + Activity")]
    Flight_Hotel_Car_Rail_Transfer_Activity = 1 << 63
}
