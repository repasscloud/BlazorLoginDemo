using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BlazorLoginDemo.Shared.Models.Kernel.Platform;
using BlazorLoginDemo.Shared.Validation;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Policies;

public class TravelPolicy
{
    [Key]
    [MaxLength(14)]
    public string Id { get; set; } = Nanoid.Generate(size: 14);

    public required string PolicyName { get; set; }
    public required string OrganizationUnifiedId { get; set; }

    // --- Financial details ---------------------------------------------------
    [Required]
    [CurrencyTypeValidation]
    [RegularExpression(@"^[A-Z]{3}$", ErrorMessage = "Currency must be exactly 3 uppercase letters.")]
    [JsonPropertyName("currency")]
    [DefaultValue("AUD")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public required string DefaultCurrencyCode { get; set; } = "AUD";

    /// <summary>
    /// Blanket maximum price (base fare + mandatory taxes/fees). 0 = "no blanket cap".
    /// </summary>
    public decimal MaxFlightPrice { get; set; } = 0m;

    // --- Flight particulars --------------------------------------------------
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public string DefaultFlightSeating { get; set; } = "ECONOMY";

    /// <summary>
    /// Blanket maximum cabin allowed (e.g., ECONOMY, PREMIUM_ECONOMY, BUSINESS).
    /// This is used when no per-threshold cabin is set.
    /// </summary>
    [CabinTypeValidation]
    [DefaultValue("ECONOMY")]
    public string MaxFlightSeating { get; set; } = "ECONOMY";

    public string[] IncludedAirlineCodes { get; set; } = Array.Empty<string>();
    public string[] ExcludedAirlineCodes { get; set; } = Array.Empty<string>();

    [CoverageTypeValidation]
    [DefaultValue("MOST_SEGMENTS")]
    public string CabinClassCoverage { get; set; } = "MOST_SEGMENTS";

    /// <summary>
    /// If true, prefer/require nonstop (depending on how you enforce it in search).
    /// </summary>
    public bool? NonStopFlight { get; set; }

    // --- Booking window / time rules ----------------------------------------
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableFrom { get; set; }  // Local time. hh:mm:ss

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [RegularExpression(@"^(0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Time must be in the format hh:mm:ss.")]
    public string? FlightBookingTimeAvailableTo { get; set; }    // Local time. hh:mm:ss

    public bool? EnableSaturdayFlightBookings { get; set; }
    public bool? EnableSundayFlightBookings { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? DefaultCalendarDaysInAdvanceForFlightBooking { get; set; }

    [JsonIgnore] // Prevent circular reference during serialization.
    public OrganizationUnified Organization { get; set; } = default!;

    // --- Geography allow/deny lists -----------------------------------------
    public ICollection<Region> Regions { get; set; } = new List<Region>();
    public ICollection<Continent> Continents { get; set; } = new List<Continent>();
    public ICollection<Country> Countries { get; set; } = new List<Country>();

    // Exclusions when a broad area is turned on (e.g., APAC but exclude CN).
    public ICollection<TravelPolicyDisabledCountry> DisabledCountries { get; set; } = new List<TravelPolicyDisabledCountry>();

    // --- Airline CSV helpers (UI convenience, not stored) --------------------
    [NotMapped]
    public string IncludedAirlineCodesCsv
    {
        get => string.Join(", ", IncludedAirlineCodes);
        set
        {
            IncludedAirlineCodes = string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim().ToUpperInvariant())
                       .Distinct()
                       .ToArray();
        }
    }

    [NotMapped]
    public string ExcludedAirlineCodesCsv
    {
        get => string.Join(", ", ExcludedAirlineCodes);
        set
        {
            ExcludedAirlineCodes = string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(s => s.Trim().ToUpperInvariant())
                       .Distinct()
                       .ToArray();
        }
    }

    // =========================================================================
    // Per-duration thresholds (6h / 8h / 10h / 14h)
    // =========================================================================
    // NOTE: All are optional. If null/zero, the blanket setting applies.

    // -- Max cabin by duration --
    [CabinTypeValidation] public string? MaxFlightSeatingAt6Hours  { get; set; }
    [CabinTypeValidation] public string? MaxFlightSeatingAt8Hours  { get; set; }
    [CabinTypeValidation] public string? MaxFlightSeatingAt10Hours { get; set; }
    [CabinTypeValidation] public string? MaxFlightSeatingAt14Hours { get; set; }

    // -- Max price by duration --
    // Use nullable so "unset" cleanly falls back to blanket MaxFlightPrice.
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxFlightPriceAt6Hours  { get; set; }
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxFlightPriceAt8Hours  { get; set; }
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxFlightPriceAt10Hours { get; set; }
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxFlightPriceAt14Hours { get; set; }

    // =========================================================================
    // Helpers the engine/OBT/TMC layer can call to "auto-calc" effective rules
    // =========================================================================

    /// <summary>
    /// Gets the effective maximum cabin based on scheduled block time.
    /// Falls back to blanket MaxFlightSeating, then DefaultFlightSeating.
    /// Valid values rely on your CabinTypeValidation (e.g., ECONOMY, PREMIUM_ECONOMY, BUSINESS).
    /// </summary>
    public string EffectiveCabinFor(TimeSpan blockTime)
    {
        var h = blockTime.TotalHours;

        // Pick the highest threshold not exceeding h, if set; else fall back.
        string? thresholdCabin =
            (h >= 14 && !string.IsNullOrWhiteSpace(MaxFlightSeatingAt14Hours)) ? MaxFlightSeatingAt14Hours
          : (h >= 10 && !string.IsNullOrWhiteSpace(MaxFlightSeatingAt10Hours)) ? MaxFlightSeatingAt10Hours
          : (h >=  8 && !string.IsNullOrWhiteSpace(MaxFlightSeatingAt8Hours )) ? MaxFlightSeatingAt8Hours
          : (h >=  6 && !string.IsNullOrWhiteSpace(MaxFlightSeatingAt6Hours )) ? MaxFlightSeatingAt6Hours
          : null;

        return thresholdCabin ?? MaxFlightSeating ?? DefaultFlightSeating;
    }

    /// <summary>
    /// Gets the effective maximum price cap based on scheduled block time.
    /// Returns null when there is no cap (neither threshold nor blanket set).
    /// </summary>
    public decimal? EffectivePriceCapFor(TimeSpan blockTime)
    {
        var h = blockTime.TotalHours;

        decimal? thresholdCap =
            (h >= 14 && MaxFlightPriceAt14Hours.HasValue) ? MaxFlightPriceAt14Hours
          : (h >= 10 && MaxFlightPriceAt10Hours.HasValue) ? MaxFlightPriceAt10Hours
          : (h >=  8 && MaxFlightPriceAt8Hours.HasValue ) ? MaxFlightPriceAt8Hours
          : (h >=  6 && MaxFlightPriceAt6Hours.HasValue ) ? MaxFlightPriceAt6Hours
          : null;

        if (thresholdCap.HasValue) return thresholdCap.Value;
        if (MaxFlightPrice > 0m)   return MaxFlightPrice;
        return null; // no cap at all
    }
}

// USAGE
// var duration = TimeSpan.FromHours(9.5);
// var cabin    = policy.EffectiveCabinFor(duration);      // e.g. "PREMIUM_ECONOMY"
// var cap      = policy.EffectivePriceCapFor(duration);   // e.g. 3500m or null (no cap)

// How it behaves (auto-calc rules)
// - For a 9.5h flight:
//    - EffectiveCabinFor(9.5h) checks 14→10→8→6; picks 8h bucket if set; else blanket.
//    - EffectivePriceCapFor(9.5h) same logic; if all threshold caps null, uses blanket MaxFlightPrice when >0, otherwise no cap.
//    - For a 15h flight: the 14h bucket wins (if set).
// - This is intentionally simple so the booking logic just calls these two methods and doesn’t care how you configured the policy.