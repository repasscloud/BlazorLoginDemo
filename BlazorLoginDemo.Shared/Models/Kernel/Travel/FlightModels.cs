using System.ComponentModel.DataAnnotations.Schema;
namespace BlazorLoginDemo.Shared.Models.Kernel.Travel;

public sealed class FlightOption
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";

    public DateTime DepartTime { get; set; }
    public DateTime ArriveTime { get; set; }

    public decimal Price { get; set; } = 0m;
    public string Currency { get; set; } = "AUD";

    public string Cabin { get; set; } = "";
    public int Stops { get; set; }

    // applies to the whole option
    public Amenities Amenities { get; set; } = new();

    // store as jsonb
    [Column(TypeName = "jsonb")]
    public List<FlightLeg> Legs { get; set; } = new();

    public string BaggageText { get; set; } = "";
    public string ChangePolicy { get; set; } = "";
    public string RefundPolicy { get; set; } = "";
    public string SeatPolicy { get; set; } = "";

    public bool IsOpen { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public 

    // view-only
    [NotMapped]
    public List<Carrier> DisplayCarriers =>
        Legs.Select(l => l.Carrier).Distinct(new CarrierCodeComparer()).ToList();

    [NotMapped]
    public string TotalDurationText
    {
        get
        {
            var span = ArriveTime - DepartTime;
            return $"{(int)span.TotalHours}h {span.Minutes:D2}m";
        }
    }

    [NotMapped]
    public IEnumerable<InclusionBadge> InclusionBadges
    {
        get
        {
            var list = new List<InclusionBadge>();

            // Connectivity
            if (Amenities.Wifi)
                list.Add(new InclusionBadge { Icon = "bi bi-wifi", Label = "Wi-Fi", BadgeClass = "badge-connectivity" });
            if (Amenities.Power)
                list.Add(new InclusionBadge { Icon = "bi bi-lightning-charge", Label = "Power", BadgeClass = "badge-connectivity" });
            if (Amenities.Usb)
                list.Add(new InclusionBadge { Icon = "bi bi-usb-symbol", Label = "USB", BadgeClass = "badge-connectivity" });
            if (Amenities.Ife)
                list.Add(new InclusionBadge { Icon = "bi bi-tv", Label = "IFE", BadgeClass = "badge-connectivity" });

            // Seating
            if (Amenities.ExtraLegroom)
                list.Add(new InclusionBadge { Icon = "bi bi-arrows-expand", Label = "Extra legroom", BadgeClass = "badge-seating" });
            if (Amenities.LieFlat)
                list.Add(new InclusionBadge { Icon = "bi bi-moon-stars", Label = "Lie-flat", BadgeClass = "badge-seating" });

            // Catering
            if (Amenities.Meal)
                list.Add(new InclusionBadge { Icon = "bi bi-cup-hot", Label = "Meal", BadgeClass = "badge-catering" });
            if (Amenities.Alcohol)
                list.Add(new InclusionBadge { Icon = "bi bi-cup-straw", Label = "Alcohol", BadgeClass = "badge-catering" });

            // Ground/Priority
            if (Amenities.PriorityBoarding)
                list.Add(new InclusionBadge { Icon = "bi bi-rocket-takeoff", Label = "Priority boarding", BadgeClass = "badge-priority" });

            // Baggage
            if (Amenities.CheckedBag)
                list.Add(new InclusionBadge { Icon = "bi bi-suitcase2", Label = "Checked bag", BadgeClass = "badge-baggage" });

            return list;
        }
    }

    [NotMapped]
    public string SearchText
    {
        get
        {
            var parts = new List<string> { Origin, Destination, Cabin };
            parts.AddRange(
                Legs.Select(l => $"{l.Carrier.Name} {l.Carrier.Code} {l.FlightNumber} {l.Origin} {l.Destination}")
            );
            return string.Join(" ", parts);
        }
    }

    private sealed class CarrierCodeComparer : IEqualityComparer<Carrier>
    {
        public bool Equals(Carrier? x, Carrier? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x is null || y is null) return false;
            return string.Equals(x.Code, y.Code, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Carrier obj) =>
            obj.Code?.ToUpperInvariant().GetHashCode() ?? 0;
    }
}

public record Carrier(string Code, string Name, string LogoUrl);

public class Layover
{
    public string Airport { get; set; } = "";
    public int Minutes { get; set; }

    [NotMapped]
    public string DurationText => $"{Minutes / 60}h {Minutes % 60:D2}m";
}

public class FlightLeg
{
    public Carrier Carrier { get; set; } = new("XX", "Carrier", "");
    public string FlightNumber { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public DateTime Depart { get; set; }
    public DateTime Arrive { get; set; }
    public string Equipment { get; set; } = "";
    public string SeatLayout { get; set; } = "";
    public string CabinClass { get; set; } = "";
    public Amenities Amenities { get; set; } = new();
    public Layover? Layover { get; set; }

    [NotMapped]
    public TimeSpan Duration => Arrive - Depart;

    [NotMapped]
    public string DurationText => $"{(int)Duration.TotalHours}h {Duration.Minutes:D2}m";

    [NotMapped]
    public IEnumerable<string> AmenityLabels
    {
        get
        {
            var list = new List<string>();
            if (Amenities.Wifi) list.Add("Wi-Fi");
            if (Amenities.Power) list.Add("Power");
            if (Amenities.Usb) list.Add("USB");
            if (Amenities.Ife) list.Add("IFE");
            if (Amenities.Meal) list.Add("Meal");
            if (Amenities.LieFlat) list.Add("Lie-flat");
            return list;
        }
    }
}

public class Amenities
{
    public bool Wifi { get; set; }
    public bool Power { get; set; }
    public bool Usb { get; set; }
    public bool Ife { get; set; }
    public bool Meal { get; set; }
    public bool LieFlat { get; set; }

    public bool ExtraLegroom { get; set; }
    public bool Lounge { get; set; }
    public bool PriorityBoarding { get; set; }
    public bool CheckedBag { get; set; }
    public bool Alcohol { get; set; }
}

public class InclusionBadge
{
    public string Icon { get; set; } = "";
    public string Label { get; set; } = "";
    public string BadgeClass { get; set; } = "";
}


public class AmenityQuery : Amenities { }

public static class DemoData
{
    static readonly Carrier QF = new("QF", "Qantas", "https://static.tripcdn.com/packages/flight/airline-logo/latest/airline_logo/3x/qf.webp");
    static readonly Carrier SQ = new("SQ", "Singapore Airlines", "https://raw.githubusercontent.com/repasscloud/IATAScraper/refs/heads/main/airline_vectors/SQ.svg");
    static readonly Carrier CA = new("CA", "Air China", "https://static.tripcdn.com/packages/flight/airline-logo/latest/airline_logo/3x/ca.webp");

    public static List<FlightOption> Create()
    {
        var baseDate = new DateTime(2025, 11, 10);

        var qfSqHybrid = new FlightOption {
            Origin="SYD", Destination="LHR",
            DepartTime = baseDate.AddHours(20).AddMinutes(55),
            ArriveTime = baseDate.AddDays(1).AddHours(6).AddMinutes(15),
            Price = 5480, Currency = "AUD", Cabin="Business", Stops=1,
            Amenities = new Amenities{ Wifi=true, Power=true, Usb=true, Ife=true, Meal=true, LieFlat=true },
            BaggageText="2×32kg", ChangePolicy="Flexible", RefundPolicy="Partial", SeatPolicy="Choose later",
            Legs = new List<FlightLeg> {
                new FlightLeg {
                    Carrier = QF, FlightNumber="QF1", Origin="SYD", Destination="SIN",
                    Depart = baseDate.AddHours(20).AddMinutes(55),
                    Arrive = baseDate.AddDays(1).AddHours(2).AddMinutes(10),
                    Equipment="A380", SeatLayout="1-2-1",
                    Amenities = new Amenities { Wifi=true,  Power=true,  Usb=true,  Ife=true,  Meal=true,  LieFlat=false, ExtraLegroom=false, Lounge=false, PriorityBoarding=false, CheckedBag=true,  Alcohol=false },
                    Layover = new Layover{ Airport="SIN", Minutes=75 }
                },
                new FlightLeg {
                    Carrier = SQ, FlightNumber="SQ334", Origin="SIN", Destination="LHR",
                    Depart = baseDate.AddDays(1).AddHours(3).AddMinutes(25),
                    Arrive = baseDate.AddDays(1).AddHours(6).AddMinutes(15),
                    Equipment="A380", SeatLayout="1-2-1",
                    Amenities = new Amenities { Wifi=false, Power=false, Usb=true,  Ife=false, Meal=false, LieFlat=false, ExtraLegroom=false, Lounge=false, PriorityBoarding=false, CheckedBag=false, Alcohol=false },
                }
            }
        };

        var sqNonstop = new FlightOption {
            Origin="SYD", Destination="SIN",
            DepartTime = baseDate.AddHours(9).AddMinutes(20),
            ArriveTime = baseDate.AddHours(15).AddMinutes(5),
            Price = 1590, Currency = "AUD", Cabin="Premium Economy", Stops=0,
            Amenities = new Amenities{ Wifi=true, Power=true, Ife=true, Meal=true },
            BaggageText="2×23kg", ChangePolicy="Fee applies", RefundPolicy="No", SeatPolicy="Preselect",
            Legs = new List<FlightLeg> {
                new FlightLeg {
                    Carrier = SQ, FlightNumber="SQ232", Origin="SYD", Destination="SIN",
                    Depart = baseDate.AddHours(9).AddMinutes(20),
                    Arrive = baseDate.AddHours(15).AddMinutes(5),
                    Equipment="A350", SeatLayout="2-4-2",
                    Amenities = new Amenities { Wifi=true,  Power=true,  Usb=true,  Ife=true,  Meal=false, LieFlat=false, ExtraLegroom=true,  Lounge=false, PriorityBoarding=true,  CheckedBag=true,  Alcohol=false },
                }
            }
        };

        var caOneStop = new FlightOption {
            Origin="SYD", Destination="LHR",
            DepartTime = baseDate.AddHours(8).AddMinutes(10),
            ArriveTime = baseDate.AddDays(1).AddMinutes(45),
            Price = 1190, Currency = "AUD", Cabin="First", Stops=1,
            Amenities = new Amenities{ Ife=true, Meal=true },
            BaggageText="1×23kg", ChangePolicy="No", RefundPolicy="No", SeatPolicy="Auto-assign",
            Legs = new List<FlightLeg> {
                new FlightLeg {
                    Carrier = CA, FlightNumber="CA176", Origin="SYD", Destination="PEK",
                    Depart = baseDate.AddHours(8).AddMinutes(10),
                    Arrive = baseDate.AddHours(18).AddMinutes(30),
                    Equipment="A330", SeatLayout="2-4-2",
                    Amenities = new Amenities { Wifi=true,  Power=true,  Usb=true,  Ife=true,  Meal=true,  LieFlat=false, ExtraLegroom=true,  Lounge=false, PriorityBoarding=true,  CheckedBag=true,  Alcohol=true  },
                    Layover = new Layover{ Airport="PEK", Minutes=130 }
                },
                new FlightLeg {
                    Carrier = CA, FlightNumber="CA937", Origin="PEK", Destination="LHR",
                    Depart = baseDate.AddHours(20).AddMinutes(40),
                    Arrive = baseDate.AddDays(1).AddMinutes(45),
                    Equipment="B777", SeatLayout="3-3-3",
                    Amenities = new Amenities { Wifi=true,  Power=true,  Usb=true,  Ife=true,  Meal=true,  LieFlat=false, ExtraLegroom=true,  Lounge=true,  PriorityBoarding=true,  CheckedBag=true,  Alcohol=true  }
                }
            }
        };

        var qfDomestic = new FlightOption {
            Origin="SYD", Destination="PER",
            DepartTime = baseDate.AddHours(13).AddMinutes(5),
            ArriveTime = baseDate.AddHours(16).AddMinutes(25),
            Price = 420, Currency = "AUD", Cabin="Economy", Stops=0,
            Amenities = new Amenities { Wifi=true,  Power=true,  Usb=true,  Ife=true,  Meal=true,  LieFlat=true,  ExtraLegroom=true,  Lounge=true,  PriorityBoarding=true,  CheckedBag=true,  Alcohol=true  },
            BaggageText="1×23kg", ChangePolicy="Fee applies", RefundPolicy="No", SeatPolicy="Auto-assign",
            Legs = new List<FlightLeg> {
                new FlightLeg {
                    Carrier = QF, FlightNumber="QF641", Origin="SYD", Destination="PER",
                    Depart = baseDate.AddHours(13).AddMinutes(5),
                    Arrive = baseDate.AddHours(16).AddMinutes(25),
                    Equipment="B737", SeatLayout="3-3",

                }
            }
        };

        return new List<FlightOption> { qfSqHybrid, sqNonstop, caOneStop, qfDomestic };
    }
}
