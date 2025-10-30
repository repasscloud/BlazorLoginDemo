using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Cinturon360.Shared.Validation;
using NanoidDotNet;

namespace Cinturon360.Shared.Models.Policies;

public class EphemeralTravelPolicy
{
    [Key]
    [MaxLength(14)]
    public string Id { get; set; } = Nanoid.Generate(size: 14);

    // --- Auditing ---------------------------------------------------------------
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    public string CreatedByUserId { get; set; } = null!;
    public required string PolicyName { get; set; }
    public required string OrganizationUnifiedId { get; set; }

    // --- Financial details ---------------------------------------------------
    [MaxLength(3)] public required string DefaultCurrencyCode { get; set; } = "AUD";

    [Column(TypeName = "numeric(14,2)")]
    public decimal MaxFlightPrice { get; set; } = 0m;

    // --- Policy effective window -------------------------------------------------
    public DateTime? EffectiveFromUtc { get; set; }
    public DateTime? ExpiresOnUtc { get; set; }

    // --- Flight particulars --------------------------------------------------
    public string DefaultFlightSeating { get; set; } = "ECONOMY";
    public string MaxFlightSeating { get; set; } = "ECONOMY";

    public string[] IncludedAirlineCodes { get; set; } = Array.Empty<string>();
    public string[] ExcludedAirlineCodes { get; set; } = Array.Empty<string>();

    public string CabinClassCoverage { get; set; } = "MOST_SEGMENTS";

    public bool? NonStopFlight { get; set; }

    // --- Accommodation ----------------------------------------------------------
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxHotelNightlyRate { get; set; }
    public string? DefaultHotelRoomType { get; set; }            // e.g., "STANDARD", "DELUXE"
    public string? MaxHotelRoomType { get; set; }                 // upper bound if needed
    public string[] IncludedHotelChains { get; set; } = Array.Empty<string>();
    public string[] ExcludedHotelChains { get; set; } = Array.Empty<string>();
    public string? HotelBookingTimeAvailableFrom { get; set; }
    public string? HotelBookingTimeAvailableTo { get; set; }
    public bool? EnableSaturdayHotelBookings { get; set; }
    public bool? EnableSundayHotelBookings { get; set; }

    [NotMapped]
    public string IncludedHotelChainsCsv
    {
        get => string.Join(", ", IncludedHotelChains);
        set => IncludedHotelChains = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }
    [NotMapped]
    public string ExcludedHotelChainsCsv
    {
        get => string.Join(", ", ExcludedHotelChains);
        set => ExcludedHotelChains = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    // --- Taxi / Ride-hail -------------------------------------------------------
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxTaxiFarePerRide { get; set; }
    public string[] IncludedTaxiVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedTaxiVendors { get; set; } = Array.Empty<string>();
    public decimal? MaxTaxiSurgeMultiplier { get; set; }          // e.g., 1.5 = 150%

    [NotMapped]
    public string IncludedTaxiVendorsCsv
    {
        get => string.Join(", ", IncludedTaxiVendors);
        set => IncludedTaxiVendors = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }
    [NotMapped]
    public string ExcludedTaxiVendorsCsv
    {
        get => string.Join(", ", ExcludedTaxiVendors);
        set => ExcludedTaxiVendors = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    // --- Train ------------------------------------------------------------------
    public string? DefaultTrainClass { get; set; }
    public string? MaxTrainClass { get; set; }
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxTrainPrice { get; set; }
    public string[] IncludedRailOperators { get; set; } = Array.Empty<string>();
    public string[] ExcludedRailOperators { get; set; } = Array.Empty<string>();

    [NotMapped]
    public string IncludedRailOperatorsCsv
    {
        get => string.Join(", ", IncludedRailOperators);
        set => IncludedRailOperators = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }
    [NotMapped]
    public string ExcludedRailOperatorsCsv
    {
        get => string.Join(", ", ExcludedRailOperators);
        set => ExcludedRailOperators = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    // --- Hire car ---------------------------------------------------------------
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxCarHireDailyRate { get; set; }
    public string[] AllowedCarHireClasses { get; set; } = Array.Empty<string>();
    public string[] IncludedCarHireVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedCarHireVendors { get; set; } = Array.Empty<string>();
    public bool? RequireInclusiveInsurance { get; set; }
    public string? DefaultCarClass { get; set; }
    public string? MaxCarClass { get; set; }
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxCarDailyRate { get; set; }

    [NotMapped]
    public string AllowedCarHireClassesCsv
    {
        get => string.Join(", ", AllowedCarHireClasses);
        set => AllowedCarHireClasses = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    [NotMapped]
    public string IncludedCarHireVendorsCsv
    {
        get => string.Join(", ", IncludedCarHireVendors);
        set => IncludedCarHireVendors = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    [NotMapped]
    public string ExcludedCarHireVendorsCsv
    {
        get => string.Join(", ", ExcludedCarHireVendors);
        set => ExcludedCarHireVendors = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    // --- Bus / Coach ------------------------------------------------------------
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxBusFarePerTicket { get; set; }
    public string[] IncludedBusOperators { get; set; } = Array.Empty<string>();
    public string[] ExcludedBusOperators { get; set; } = Array.Empty<string>();

    [NotMapped]
    public string IncludedBusOperatorsCsv
    {
        get => string.Join(", ", IncludedBusOperators);
        set => IncludedBusOperators = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }
    [NotMapped]
    public string ExcludedBusOperatorsCsv
    {
        get => string.Join(", ", ExcludedBusOperators);
        set => ExcludedBusOperators = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    // --- SIM / eSIM -------------------------------------------------------------
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxSimBundlePrice { get; set; }
    public decimal? MinSimDataGb { get; set; }                     // e.g., 5 = 5GB
    public int? MinSimValidityDays { get; set; }
    public string[] IncludedSimVendors { get; set; } = Array.Empty<string>();
    public string[] ExcludedSimVendors { get; set; } = Array.Empty<string>();

    [NotMapped]
    public string IncludedSimVendorsCsv
    {
        get => string.Join(", ", IncludedSimVendors);
        set => IncludedSimVendors = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }
    
    [NotMapped]
    public string ExcludedSimVendorsCsv
    {
        get => string.Join(", ", ExcludedSimVendors);
        set => ExcludedSimVendors = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }

    // --- Holiday activity / Excursions -----------------------------------------
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxActivityPricePerPerson { get; set; }
    public string[] IncludedActivityProviders { get; set; } = Array.Empty<string>();
    public string[] ExcludedActivityProviders { get; set; } = Array.Empty<string>();
    public bool? AllowHighRiskActivities { get; set; }             // e.g., bungee, scuba, etc.

    [NotMapped]
    public string IncludedActivityProvidersCsv
    {
        get => string.Join(", ", IncludedActivityProviders);
        set => IncludedActivityProviders = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }
    
    [NotMapped]
    public string ExcludedActivityProvidersCsv
    {
        get => string.Join(", ", ExcludedActivityProviders);
        set => ExcludedActivityProviders = string.IsNullOrWhiteSpace(value)
            ? Array.Empty<string>()
            : value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToArray();
    }


    // --- Booking window / time rules ----------------------------------------
    public string? FlightBookingTimeAvailableFrom { get; set; }  // Local time. hh:mm:ss
    public string? FlightBookingTimeAvailableTo { get; set; }    // Local time. hh:mm:ss

    public bool? EnableSaturdayFlightBookings { get; set; }
    public bool? EnableSundayFlightBookings { get; set; }

    public int? DefaultCalendarDaysInAdvanceForFlightBooking { get; set; }


    // --- Geography allow/deny lists -----------------------------------------
    public int[] RegionIds { get; set; } = Array.Empty<int>();
    public int[] ContinentIds { get; set; } = Array.Empty<int>();
    public int[] CountryIds { get; set; } = Array.Empty<int>();

    // Exclusions when a broad area is turned on (e.g., APAC but exclude CN).
    public int[] DisabledCountryIds { get; set; } = Array.Empty<int>();


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
    [CabinTypeValidation] public string? MaxFlightSeatingAt6Hours { get; set; }
    [CabinTypeValidation] public string? MaxFlightSeatingAt8Hours { get; set; }
    [CabinTypeValidation] public string? MaxFlightSeatingAt10Hours { get; set; }
    [CabinTypeValidation] public string? MaxFlightSeatingAt14Hours { get; set; }

    // -- Max price by duration --
    // Use nullable so "unset" cleanly falls back to blanket MaxFlightPrice.
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxFlightPriceAt6Hours { get; set; }
    [Column(TypeName = "numeric(14,2)")] public decimal? MaxFlightPriceAt8Hours { get; set; }
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
          : (h >= 8 && !string.IsNullOrWhiteSpace(MaxFlightSeatingAt8Hours)) ? MaxFlightSeatingAt8Hours
          : (h >= 6 && !string.IsNullOrWhiteSpace(MaxFlightSeatingAt6Hours)) ? MaxFlightSeatingAt6Hours
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
          : (h >= 8 && MaxFlightPriceAt8Hours.HasValue) ? MaxFlightPriceAt8Hours
          : (h >= 6 && MaxFlightPriceAt6Hours.HasValue) ? MaxFlightPriceAt6Hours
          : null;

        if (thresholdCap.HasValue) return thresholdCap.Value;
        if (MaxFlightPrice > 0m) return MaxFlightPrice;
        return null; // no cap at all
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


    // =========================================================================
    // Approval workflow rules
    // =========================================================================

    // If true and the fare is within the effective policy limit, auto-approve and
    // ignore manager/L1/L2/L3/Billing-Contact (to-policy) requirements.
    [DefaultValue(false)]
    public bool AutoApproveToPolicyLimit { get; set; } = false;

    // If true and within the effective policy limit, require Manager approval
    // (unless AutoApproveToPolicyLimit short-circuits it).
    [DefaultValue(false)]
    public bool RequireManagerApprovalToPolicyLimit { get; set; } = false;

    // L1/L2/L3 approvals:
    // - When the corresponding "*ApprovalRequired" is true AND an Amount is set,
    //   the level is required only when FareTotal > Amount.
    // - When "*ApprovalRequired" is true and Amount is null, the level is always
    //   required (within the policy window), regardless of price.
    [DefaultValue(false)] public bool L1ApprovalRequired { get; set; } = false;
    [Column(TypeName = "numeric(14,2)")] public decimal? L1ApprovalAmount { get; set; } = null;
    [DefaultValue(false)] public bool L2ApprovalRequired { get; set; } = false;

    [Column(TypeName = "numeric(14,2)")] public decimal? L2ApprovalAmount { get; set; } = null;

    [DefaultValue(false)] public bool L3ApprovalRequired { get; set; } = false;
    [Column(TypeName = "numeric(14,2)")] public decimal? L3ApprovalAmount { get; set; } = null;

    // Billing Contact rules
    // - ToPolicyLimit: even when within policy, also get Billing Contact’s approval.
    // - AbovePolicyLimit: used when a TMC/Platform override is attempting to book
    //   above the effective limit; if true, require Billing Contact approval.
    [DefaultValue(false)] public bool BillingContactApprovalToPolicyLimit { get; set; } = false;

    [DefaultValue(false)] public bool BillingContactApprovalAbovePolicyLimit { get; set; } = false;

    /// <summary>
    /// Returns the approval targets that must sign off for a proposed fare,
    /// based on this policy and the effective (duration-aware) price cap.
    /// 
    /// Arguments:
    /// - fareTotal: the proposed total (base + mandatory taxes/fees)
    /// - blockTime: scheduled block time (used to compute the effective cap)
    /// - isOverrideAbovePolicy: true only when an authorized TMC/Platform flow
    ///   is *attempting* to book above the policy limit.
    /// - hasManagerAssigned: whether the traveler has a manager (used only if
    ///   manager approval is required). If false, the caller should decide how
    ///   to route Manager approvals (fallback group, etc.).
    /// </summary>
    public ApprovalDecision EvaluateApprovals(
        decimal fareTotal,
        TimeSpan blockTime,
        bool isOverrideAbovePolicy,
        bool hasManagerAssigned
    )
    {
        var required = new HashSet<ApproverTarget>();

        // Determine the effective cap for this duration
        var cap = EffectivePriceCapFor(blockTime);
        var withinPolicy = !cap.HasValue || fareTotal <= cap.Value;

        // 1) Auto-approve short-circuit (within policy only)
        if (withinPolicy && AutoApproveToPolicyLimit)
        {
            return new ApprovalDecision(
                WithinPolicy: true,
                AutoApproved: true,
                RequiredTargets: Array.Empty<ApproverTarget>()
            );
        }

        // 2) If *not* within policy:
        //    - If no override path, the caller should typically block upstream.
        //    - If override path is active, require Billing Contact if configured.
        if (!withinPolicy)
        {
            if (isOverrideAbovePolicy && BillingContactApprovalAbovePolicyLimit)
                required.Add(ApproverTarget.BillingContact);

            var orderedAbove = required
                .OrderBy(t => t == ApproverTarget.Manager ? 0
                        : t == ApproverTarget.Level1  ? 1
                        : t == ApproverTarget.Level2  ? 2
                        : t == ApproverTarget.Level3  ? 3
                        : 4) // BillingContact last
                .ToArray();

            return new ApprovalDecision(
                WithinPolicy: false,
                AutoApproved: false,
                RequiredTargets: orderedAbove
            );
        }

        // 3) Within policy window: accumulate required approvals.

        // 3a) Manager (if configured)
        if (RequireManagerApprovalToPolicyLimit && hasManagerAssigned)
            required.Add(ApproverTarget.Manager);

        // 3b) Billing Contact even within policy (if configured)
        if (BillingContactApprovalToPolicyLimit)
            required.Add(ApproverTarget.BillingContact);

        // 3c) L1/L2/L3
        // If the boolean is true and amount is null => always required (within policy).
        // If the boolean is true and amount is set  => required only when fare >= amount.
        if (L1ApprovalRequired && (!L1ApprovalAmount.HasValue || fareTotal >= L1ApprovalAmount.Value))
            required.Add(ApproverTarget.Level1);

        if (L2ApprovalRequired && (!L2ApprovalAmount.HasValue || fareTotal >= L2ApprovalAmount.Value))
            required.Add(ApproverTarget.Level2);

        if (L3ApprovalRequired && (!L3ApprovalAmount.HasValue || fareTotal >= L3ApprovalAmount.Value))
            required.Add(ApproverTarget.Level3);

        var ordered = required
            .OrderBy(t => t == ApproverTarget.Manager ? 0
                    : t == ApproverTarget.Level1  ? 1
                    : t == ApproverTarget.Level2  ? 2
                    : t == ApproverTarget.Level3  ? 3
                    : 4) // BillingContact last
            .ToArray();

        return new ApprovalDecision(
            WithinPolicy: true,
            AutoApproved: false,
            RequiredTargets: ordered
        );
    }

    /// <summary>
    /// Convenience wrapper if you only want the targets.
    /// </summary>
    public IReadOnlyCollection<ApproverTarget> GetApprovalTargets(
        decimal fareTotal,
        TimeSpan blockTime,
        bool isOverrideAbovePolicy,
        bool hasManagerAssigned
    )
        => EvaluateApprovals(fareTotal, blockTime, isOverrideAbovePolicy, hasManagerAssigned).RequiredTargets;

    /// <summary>
    /// Convenience wrapper to check if it will auto-approve (within policy).
    /// </summary>
    public bool ShouldAutoApprove(decimal fareTotal, TimeSpan blockTime)
    {
        var cap = EffectivePriceCapFor(blockTime);
        var withinPolicy = !cap.HasValue || fareTotal <= cap.GetValueOrDefault();
        return withinPolicy && AutoApproveToPolicyLimit;
    }
}
