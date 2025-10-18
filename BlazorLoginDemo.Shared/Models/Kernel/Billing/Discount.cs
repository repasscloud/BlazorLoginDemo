using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.Billing
{
    [Index(nameof(DiscountCode), IsUnique = true)]
    public class Discount
    {
        [Key]
        [MaxLength(30)]
        public string Id { get; set; } = Nanoid.Generate(size: 21);

        [MaxLength(30)]
        public string? ParentOrgId { get; set; }

        [MaxLength(30)]
        [Required]
        public required string DiscountCode { get; set; } = default!;

        [Required, MaxLength(120)]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        public DiscountType Type { get; set; } = DiscountType.Percentage;

        /// <summary>
        /// Percentage: 0–100 (store as whole percent, e.g., 20 = 20%).
        /// FixedAmount: currency amount in major units (e.g., 25.00).
        /// </summary>
        [Precision(18, 4)]
        public decimal Amount { get; set; } = 0m;

        /// <summary>
        /// Required when Type == FixedAmount. ISO-4217 code, e.g., "AUD".
        /// </summary>
        [MaxLength(3)]
        public string? Currency { get; set; }

        /// <summary>
        /// Where/when this discount is eligible to apply.
        /// </summary>
        [Required]
        public DiscountScope Scope { get; set; } = DiscountScope.PAYG;

        /// <summary>
        /// Optional: first-N-period discount semantics (used when Scope = FirstNPeriods).
        /// </summary>
        public int? DurationInPeriods { get; set; }

        /// <summary>
        /// Optional absolute window; evaluated in UTC.
        /// </summary>
        public DateTime? StartsAtUtc { get; set; }
        public DateTime? EndsAtUtc { get; set; }

        /// <summary>
        /// Optional seat eligibility gates; enforce in domain logic.
        /// </summary>
        public int? SeatMin { get; set; }
        public int? SeatMax { get; set; }

        /// <summary>
        /// Soft state flag for operational control (archive without deleting).
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Stripe linkage (for reconciliation).
        /// </summary>
        [MaxLength(64)]
        public string? StripeCouponId { get; set; }
        
        [MaxLength(64)]
        public string? StripePromotionCodeId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // ---- Convenience computed members ----

        [NotMapped]
        public bool IsPercentage => Type == DiscountType.Percentage;

        /// <summary>
        /// Returns 0–100 when percentage; otherwise 0.
        /// </summary>
        [NotMapped]
        public decimal PercentOff => IsPercentage ? Amount : 0m;

        /// <summary>
        /// Returns currency amount when fixed; otherwise 0.
        /// </summary>
        [NotMapped]
        public decimal FixedAmountOff => Type == DiscountType.FixedAmount ? Amount : 0m;

        /// <summary>
        /// Active if enabled and now is within optional window.
        /// </summary>
        [NotMapped]
        public bool IsActive
        {
            get
            {
                var now = DateTime.UtcNow;
                if (!IsEnabled) return false;
                if (StartsAtUtc.HasValue && now < StartsAtUtc.Value) return false;
                if (EndsAtUtc.HasValue && now > EndsAtUtc.Value) return false;
                return true;
            }
        }

        // ---- Basic guard (call in service layer before persisting/using) ----
        public void Validate()
        {
            if (Type == DiscountType.Percentage)
            {
                if (Amount < 0m || Amount > 100m)
                    throw new ValidationException("Percentage Amount must be between 0 and 100.");
                if (!string.IsNullOrEmpty(Currency))
                    throw new ValidationException("Currency must be null for percentage discounts.");
            }
            else // FixedAmount
            {
                if (Amount < 0m)
                    throw new ValidationException("Fixed Amount cannot be negative.");
                if (string.IsNullOrWhiteSpace(Currency) || Currency!.Length != 3)
                    throw new ValidationException("Currency (ISO-4217) is required for fixed-amount discounts.");
            }

            if (SeatMin.HasValue && SeatMin < 0) throw new ValidationException("SeatMin cannot be negative.");
            if (SeatMax.HasValue && SeatMax < 0) throw new ValidationException("SeatMax cannot be negative.");
            if (SeatMin.HasValue && SeatMax.HasValue && SeatMin > SeatMax)
                throw new ValidationException("SeatMin cannot exceed SeatMax.");

            if (StartsAtUtc.HasValue && EndsAtUtc.HasValue && EndsAtUtc < StartsAtUtc)
                throw new ValidationException("EndsAtUtc cannot be earlier than StartsAtUtc.");
        }
    }

    public enum DiscountType
    {
        [Display(Name = "Percentage")]
        Percentage = 1,

        [Display(Name = "Fixed Amount")]
        FixedAmount = 2
    }

    /// <summary>
    /// Scope describes WHERE and WHEN a discount is allowed to apply.
    /// Integer ranges group related scopes for easy filtering.
    /// 1xx = cadence, 2xx = application layer, 3xx = eligibility/audience,
    /// 4xx = temporal window, 5xx = currency/region, 6xx = line-type.
    /// </summary>
    public enum DiscountScope : int
    {
        // ===== 1xx: Cadence / Billing rhythm =====
        [Display(Name = "Unknown")]
        Unknown = 0,

        [Display(Name = "Pay-As-You-Go")]
        PAYG = 101,

        [Display(Name = "Monthly")]
        Monthly = 110,

        [Display(Name = "Quarterly")]
        Quarterly = 120,

        [Display(Name = "Bi-Annual")]
        BiAnnual = 130,

        [Display(Name = "Annual")]
        Annual = 140,

        [Display(Name = "Weekly")]
        Weekly = 150,

        [Display(Name = "Semi-Monthly")]
        SemiMonthly = 160,

        [Display(Name = "Biennial")]
        Biennial = 170,

        [Display(Name = "Any Recurring")]
        AnyRecurring = 199,


        // ===== 2xx: Application layer =====
        [Display(Name = "Subscription Level")]
        SubscriptionLevel = 210,

        [Display(Name = "Invoice Level")]
        InvoiceLevel = 220,

        [Display(Name = "Invoice Item Level")]
        InvoiceItemLevel = 230,

        [Display(Name = "Metered Item Only")]
        MeteredItemOnly = 240,

        [Display(Name = "One-Off Invoice Only")]
        OneOffInvoiceOnly = 250,

        [Display(Name = "Customer Level Default")]
        CustomerLevelDefault = 260,


        // ===== 3xx: Eligibility / Audience =====

        [Display(Name = "New Customer Only")]
        NewCustomerOnly = 310,

        [Display(Name = "Existing Customer Only")]
        ExistingCustomerOnly = 320,

        [Display(Name = "Specific Organization Only")]
        SpecificOrgOnly = 330,

        [Display(Name = "First Subscription Only")]
        FirstSubscriptionOnly = 340,

        [Display(Name = "Seat Threshold Min")]
        SeatThresholdMin = 350,

        [Display(Name = "Seat Threshold Max")]
        SeatThresholdMax = 351,


        // ===== 4xx: Temporal window =====
        [Display(Name = "First N Periods")]
        FirstNPeriods = 410,

        [Display(Name = "Exact Date Window")]
        ExactDateWindow = 420,

        [Display(Name = "Trial Period Only")]
        TrialPeriodOnly = 430,

        [Display(Name = "Renewal Only")]
        RenewalOnly = 440,

        [Display(Name = "Perpetuity")]
        Perpetuity = 499,


        // ===== 5xx: Currency / Region constraints =====
        [Display(Name = "Currency Fixed Amount Only")]
        CurrencyFixedAmountOnly = 510,

        [Display(Name = "Multi-Currency Percent")]
        MultiCurrencyPercent = 520,

        [Display(Name = "Region Restricted")]
        RegionRestricted = 530,

        [Display(Name = "Tax Inclusive Prices Only")]
        TaxInclusivePricesOnly = 540,

        // ===== 6xx: Line-type / Charge-type scoping =====
        [Display(Name = "Setup Fees")]
        SetupFees = 610,

        [Display(Name = "Add-Ons")]
        AddOns = 620,

        [Display(Name = "Overages")]
        Overages = 630,

        [Display(Name = "Platform Fee")]
        PlatformFee = 640,

        [Display(Name = "Adjustment Credits Only")]
        AdjustmentCreditsOnly = 650
    }
}
