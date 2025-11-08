using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cinturon360.Shared.Models.ExternalLib.Amadeus.Flight;
using NanoidDotNet;

namespace Cinturon360.Shared.Models.Kernel.Travel;

public sealed class FlightViewOption
{
    [Key]
    public string Id { get; set; } = Nanoid.Generate();

    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;

    public DateTime DepartTime { get; set; }
    public DateTime ArriveTime { get; set; }

    public decimal Price { get; set; } = 0m;
    public string Currency { get; set; } = "AUD";
    public string CurrencySymbol { get; set; } = "$";

    public List<string> Cabins { get; set; } = new();

    public int Stops { get; set; }

    // persisted as owned value object (see DbContext)
    public List<Amenity> Amenities { get; set; } = new();

    // persisted as jsonb
    [Column(TypeName = "jsonb")]
    public List<FlightLeg> Legs { get; set; } = new();

    public string BaggageText { get; set; } = string.Empty;
    public string ChangePolicy { get; set; } = string.Empty;
    public string RefundPolicy { get; set; } = string.Empty;
    public string SeatPolicy { get; set; } = string.Empty;

    public bool IsOpen { get; set; }

    [Required]
    public required string QuoteId { get; set; }

    // raw Amadeus
    [Column(TypeName = "jsonb")]
    public FlightOffer? AmadeusFlightOffer { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // VIEW-ONLY
    [NotMapped]
    public List<Carrier> DisplayCarriers =>
        Legs.Select(l => l.Carrier).Distinct(new CarrierCodeComparer()).ToList();

    [NotMapped]
    public string TotalDurationText
    {
        get
        {
            TimeSpan totalDuration = TimeSpan.Zero;
            foreach (var leg in Legs)
            {
                TimeSpan legTotalDuration = TimeSpan.Zero;
                if (leg.Layover != null)
                {
                    // include layover time
                    var layoverSpan = TimeSpan.FromMinutes(leg.Layover.Minutes);
                    legTotalDuration += layoverSpan;
                }
                totalDuration += leg.Duration;
            }
            return $"{(int)totalDuration.TotalHours}h {totalDuration.Minutes:D2}m";
        }
    }

    [NotMapped]
    public IEnumerable<InclusionBadge> InclusionBadges
    {
        get
        {
            var list = new List<InclusionBadge>();

            foreach (var a in Amenities)
            {
                // Baggage
                if (a.Type == AmenityType.BAGGAGE && a.Name.Equals("Pre-paid Baggage", StringComparison.OrdinalIgnoreCase) && a.IsActive)
                    list.Add(new InclusionBadge { Icon = "bi bi-suitcase2", Label = "Checked bag", BadgeClass = "badge-baggage" });
            }
            // if (Amenities.Wifi)
            //     list.Add(new InclusionBadge { Icon = "bi bi-wifi", Label = "Wi-Fi", BadgeClass = "badge-connectivity" });
            // if (Amenities.Power)
            //     list.Add(new InclusionBadge { Icon = "bi bi-lightning-charge", Label = "Power", BadgeClass = "badge-connectivity" });
            // if (Amenities.Usb)
            //     list.Add(new InclusionBadge { Icon = "bi bi-usb-symbol", Label = "USB", BadgeClass = "badge-connectivity" });
            // if (Amenities.Ife)
            //     list.Add(new InclusionBadge { Icon = "bi bi-tv", Label = "IFE", BadgeClass = "badge-connectivity" });

            // // Seating
            // if (Amenities.ExtraLegroom)
            //     list.Add(new InclusionBadge { Icon = "bi bi-arrows-expand", Label = "Extra legroom", BadgeClass = "badge-seating" });
            // if (Amenities.LieFlat)
            //     list.Add(new InclusionBadge { Icon = "bi bi-moon-stars", Label = "Lie-flat", BadgeClass = "badge-seating" });

            // // Catering
            // if (Amenities.Meal)
            //     list.Add(new InclusionBadge { Icon = "bi bi-cup-hot", Label = "Meal", BadgeClass = "badge-catering" });
            // if (Amenities.Alcohol)
            //     list.Add(new InclusionBadge { Icon = "bi bi-cup-straw", Label = "Alcohol", BadgeClass = "badge-catering" });

            // // Ground/Priority
            // if (Amenities.PriorityBoarding)
            //     list.Add(new InclusionBadge { Icon = "bi bi-rocket-takeoff", Label = "Priority boarding", BadgeClass = "badge-priority" });

            // // Baggage
            // if (Amenities.CheckedBag)
            //     list.Add(new InclusionBadge { Icon = "bi bi-suitcase2", Label = "Checked bag", BadgeClass = "badge-baggage" });

            return list;
        }
    }

    [NotMapped]
    public string SearchText
    {
        get
        {
            var parts = new List<string> { Origin, Destination };
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
    public string Airport { get; set; } = string.Empty;
    public int Minutes { get; set; }

    [NotMapped]
    public string DurationText => $"{Minutes / 60}h {Minutes % 60:D2}m";
}

public class FlightLeg
{
    public Carrier Carrier { get; set; } = new("XX", "Carrier", "");
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string OriginTerminal { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string DestinationTerminal { get; set; } = string.Empty;
    public DateTime Depart { get; set; }
    public DateTime Arrive { get; set; }
    public string Equipment { get; set; } = string.Empty;
    public string EquipmentName { get; set; } = string.Empty;
    public string SeatLayout { get; set; } = string.Empty;
    public string CabinClass { get; set; } = string.Empty;
    public List<Amenity> Amenities { get; set; } = new();
    public Layover? Layover { get; set; }
    public string? OperatingCarrierCode { get; set; }
    public bool IsCodeShare =>
        !string.IsNullOrWhiteSpace(OperatingCarrierCode) &&
        !string.Equals(OperatingCarrierCode, Carrier.Code, StringComparison.OrdinalIgnoreCase);

    public int CheckedBagsAllowed { get; set; } = 0;
    public int? CheckedBagsWeight { get; set; }
    public string? CheckedBagsWeightUnit { get; set; }
    public int CabinBagsAllowed { get; set; } = 0;
    public int? CabinBagsWeight { get; set; }
    public string? CabinBagsWeightUnit { get; set; }
    public TimeSpan Duration { get; set; }
    public string DurationText { get; set; } = string.Empty;

    [NotMapped]
    public IEnumerable<string> AmenityLabels
    {
        get
        {
            var list = new List<string>();
            foreach (var a in Amenities)
            {
                // Baggage
                if (a.Type == AmenityType.BAGGAGE && a.Name.Equals("Pre-paid Baggage", StringComparison.OrdinalIgnoreCase) && a.IsActive)
                    list.Add("Checked bag");
            }
            return list;
        }
    }
}

// persists as owned on FlightViewOption
public sealed class Amenity
{
    public AmenityType Type { get; set; }
    public string Name { get; set; } = string.Empty;

    //pick one of these two in UI
    public string? SvgPath { get; set; }  // eg "img/amenities/wifi.svg"
    public string? IconClass { get; set; }  // eg "bi bi-wifi

    public bool IsChargeable { get; set; } = false;
    public bool IsActive { get; set; }  // user/product specific
    public int? Quantity { get; set; }  // eg number of bags
}

public enum AmenityType
{
    UNKNOWN,
    BAGGAGE,
    BRANDED_FARES,
    MEAL,
    TRAVEL_SERVICES,
    PRE_RESERVED_SEAT,
}

// VIEW-ONLY. keep as class.
public class InclusionBadge
{
    public string Icon { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string BadgeClass { get; set; } = string.Empty;
}

// used for queries, not persisted
public sealed class AmenityQuery
{
    public bool? Wifi { get; set; }
    public bool? Power { get; set; }
    public bool? Usb { get; set; }
    public bool? Ife { get; set; }
    public bool? Meal { get; set; }
    public bool? LieFlat { get; set; }
    public bool? ExtraLegroom { get; set; }
    public bool? Lounge { get; set; }
    public bool? PriorityBoarding { get; set; }
    public bool? CheckedBag { get; set; }
    public bool? Alcohol { get; set; }
}
