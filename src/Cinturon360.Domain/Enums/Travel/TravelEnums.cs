namespace Cinturon360.Domain.Enums.Travel;

public enum CabinClass
{
    Economy       = 1,
    PremiumEconomy = 2,
    Business      = 3,
    First         = 4
}

public enum MealType
{
    Standard      = 1,
    Vegetarian    = 2,
    Vegan         = 3,
    Kosher        = 4,
    Halal         = 5,
    GlutenFree    = 6,
    LowCalorie    = 7,
    ChildMeal     = 8,
    None          = 9
}

public enum SeatType
{
    Window  = 1,
    Middle  = 2,
    Aisle   = 3
}

public enum TripType
{
    OneWay     = 1,
    RoundTrip  = 2,
    MultiCity  = 3
}

public enum TransportMode
{
    Flight = 1,
    Rail   = 2,
    Car    = 3,
    Bus    = 4
}

public enum AccommodationType
{
    Hotel          = 1,
    ApartmentHotel = 2,
    Hostel         = 3,
    Bnb            = 4
}
