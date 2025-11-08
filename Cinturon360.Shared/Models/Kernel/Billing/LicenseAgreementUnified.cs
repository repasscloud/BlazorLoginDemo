using System.ComponentModel.DataAnnotations;
using Cinturon360.Shared.Models.Kernel.Platform;
using Cinturon360.Shared.Models.Static.Billing;

namespace Cinturon360.Shared.Models.Kernel.Billing;

// ------------------------------
// LICENSE AGREEMENT (Unified) with embedded supporting types
// ------------------------------
public sealed class LicenseAgreementUnified
{
    [Key]
    [MaxLength(64)]
    public string Id { get; set; } = NanoidDotNet.Nanoid.Generate(NanoidDotNet.Nanoid.Alphabets.HexadecimalUppercase, 14);

    // Who is the agreement FOR and who created/issued it
    [Required]
    public required string OrganizationUnifiedId { get; set; } // the org that is being billed/licensed
    public OrganizationUnified Organization { get; set; } = default!;

    [Required]
    public required string CreatedByOrganizationUnifiedId { get; set; } // issuer (e.g., Platform or TMC)
    public OrganizationUnified CreatedByOrganization { get; set; } = default!;

    // Core dates
    [DataType(DataType.Date)] public DateOnly StartDate { get; set; }
    [DataType(DataType.Date)] public DateOnly ExpiryDate { get; set; }
    [DataType(DataType.Date)] public DateOnly? RenewalDate { get; set; }

    // Remittance & payment rails
    [EmailAddress]
    public string? RemittanceEmail { get; set; }
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net0;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;

    // Billing model
    public BillingType BillingType { get; set; } = BillingType.Prepaid;   // Prepaid vs Postpaid
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly; // For access fee cycle
    public bool AutoRenew { get; set; } = false;

    // Access fee (fee to access the system). If PAYG, set AccessFee to 0 and scope to PAYG
    public decimal AccessFee { get; set; } = 0m;
    public BillingPeriodScope AccessFeeScope { get; set; } = BillingPeriodScope.Monthly; // PAYG/Monthly/Quarterly/BiAnnual/Annual

    // Thresholds & financials
    public decimal AccountThreshold { get; set; } = 0m; // Limit applied according to ThresholdScope
    public BillingPeriodScope ThresholdScope { get; set; } = BillingPeriodScope.Monthly;

    public decimal TaxRate { get; set; } = 0m;          // applied during invoice generation
    public decimal? MinimumMonthlySpend { get; set; }   // optional minimum spend
    public decimal PrepaidBalance { get; set; } = 0m;   // upfront funds held on account

    public int GracePeriodDays { get; set; } = 0;       // after due date before lock

    // Discount windows (two slots: A=flat, B=percent) with period scopes
    public PeriodScopedFlatDiscount? DiscountA { get; set; }
    public PeriodScopedPercentDiscount? DiscountB { get; set; }

    // Trial period during which access/fees may be waived by invoice engine
    public DateTime? TrialEndsOnUtc { get; set; }

    // PNR & service fees (carried over and unified)
    public decimal PnrCreationFee { get; set; } = 0m;
    public decimal PnrChangeFee { get; set; } = 0m;

    public decimal FlightMarkupPercent { get; set; } = 0m;
    public decimal FlightPerItemFee { get; set; } = 0m;
    public ServiceFeeType FlightFeeType { get; set; } = ServiceFeeType.None;

    public decimal HotelMarkupPercent { get; set; } = 0m;
    public decimal HotelPerItemFee { get; set; } = 0m;
    public ServiceFeeType HotelFeeType { get; set; } = ServiceFeeType.None;

    public decimal CarMarkupPercent { get; set; } = 0m;
    public decimal CarPerItemFee { get; set; } = 0m;
    public ServiceFeeType CarFeeType { get; set; } = ServiceFeeType.None;

    public decimal RailMarkupPercent { get; set; } = 0m;
    public decimal RailPerItemFee { get; set; } = 0m;
    public ServiceFeeType RailFeeType { get; set; } = ServiceFeeType.None;

    public decimal TransferMarkupPercent { get; set; } = 0m;
    public decimal TransferPerItemFee { get; set; } = 0m;
    public ServiceFeeType TransferFeeType { get; set; } = ServiceFeeType.None;

    public decimal ActivityMarkupPercent { get; set; } = 0m;
    public decimal ActivityPerItemFee { get; set; } = 0m;
    public ServiceFeeType ActivityFeeType { get; set; } = ServiceFeeType.None;

    public decimal TravelMarkupPercent { get; set; } = 0m;
    public decimal TravelPerItemFee { get; set; } = 0m;
    public ServiceFeeType TravelFeeType { get; set; } = ServiceFeeType.None;

    // Late fees (embedded, unified)
    public LateFeeSettings LateFees { get; set; } = new();

    // TMC/Client capacity-style options (optional; can be ignored if open-table)
    public int? ClientCountLimit { get; set; }   // For TMC: max clients allowed
    public int? UserAccountLimit { get; set; }   // For Client: max user accounts

    // Status fields
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending; // rolling view of last invoice status

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // ------------------------------
    // EMBEDDED SUPPORTING TYPES
    // ------------------------------
    public enum BillingPeriodScope { PAYG = 0, Monthly = 1, Quarterly = 2, BiAnnual = 3, Annual = 4 }

    public sealed class PeriodScopedFlatDiscount
    {
        public decimal Amount { get; set; }               // flat amount
        public BillingPeriodScope Scope { get; set; }     // which cycle it applies to
        public DateTime? ExpiresOnUtc { get; set; }       // end of discount validity
    }

    public sealed class PeriodScopedPercentDiscount
    {
        public decimal Percent { get; set; }              // 0..100
        public BillingPeriodScope Scope { get; set; }
        public DateTime? ExpiresOnUtc { get; set; }
    }

    public sealed class LateFeeSettings
    {
        public int GracePeriodDays { get; set; } = 0;
        public bool UseFixedAmount { get; set; } = false;
        public decimal FixedAmount { get; set; } = 0m;          // charged once per late occurrence
        public decimal PercentOfInvoice { get; set; } = 0m;     // additive percent of unpaid amount
        public decimal MaxLateFeeCap { get; set; } = 0m;        // total cap across all late fees for the invoice
        public PaymentTerms Terms { get; set; } = PaymentTerms.Net0; // allowed terms for invoices under this agreement
    }
}
