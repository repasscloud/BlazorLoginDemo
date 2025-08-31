using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Static.Billing;
using BlazorLoginDemo.Shared.Validation;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.Billing;

public class LicenseAgreement
{
    [Key]
    [MaxLength(14)]
    public string Id { get; set; } = Nanoid.Generate(alphabet: Nanoid.Alphabets.HexadecimalUppercase, size: 14);

    [Required]
    public required string AvaClientId { get; set; }
    public PaymentTerms PaymentTerms { get; set; } = PaymentTerms.Net0;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Stripe;
    public BillingType BillingType { get; set; } = BillingType.Prepaid;
    public BillingFrequency BillingFrequency { get; set; } = BillingFrequency.Monthly;
    public string RemittanceEmail { get; set; } = string.Empty;
    public bool AutoRenew { get; set; }
    public int GracePeriodDays { get; set; }

    [MoneyPrecision]
    public decimal AccessFee { get; set; } = 0m;

    [MoneyPrecision]
    public decimal PrepaidBalance { get; set; } = 0m;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    [MaxLength(14)]
    public string? LateFeeConfigId { get; set; }

    [MoneyPrecision]
    public decimal AccountThreshold { get; set; } = 0m;

    [TaxPrecision]
    public decimal TaxRate { get; set; } = 0m;
    public DateTime? TrialEndsOn { get; set; }

    [MarkupPrecision]
    public decimal Discount { get; set; } = 0m;
    public DateTime? DiscountExpires { get; set; }

    // --- PNR Fees ---
    /// <summary>
    /// Fee charged (in CurrencyCode) per PNR (Passenger Name Record) created.
    /// </summary>
    [MoneyPrecision]
    public decimal PnrCreationFee { get; set; } = 0m;

    /// <summary>
    /// Fee charged (in CurrencyCode) per PNR modification/change.
    /// </summary>
    [MoneyPrecision]
    public decimal PnrChangeFee { get; set; } = 0m;

    // --- Flight Fees ---
    [MarkupPrecision]
    public decimal FlightMarkupPercent { get; set; } = 0m;

    [MoneyPrecision]
    public decimal FlightPerItemFee { get; set; } = 0m;
    public ServiceFeeType FlightFeeType { get; set; } = ServiceFeeType.None;

    // --- Hotel Fees ---
    [MarkupPrecision]
    public decimal HotelMarkupPercent { get; set; } = 0m;

    [MoneyPrecision]
    public decimal HotelPerItemFee { get; set; } = 0m;
    public ServiceFeeType HotelFeeType { get; set; } = ServiceFeeType.None;

    // --- Car Fees ---
    [MarkupPrecision]
    public decimal CarMarkupPercent { get; set; } = 0m;

    [MoneyPrecision]
    public decimal CarPerItemFee { get; set; } = 0m;
    public ServiceFeeType CarFeeType { get; set; } = ServiceFeeType.None;

    // --- Rail Fees ---
    [MarkupPrecision]
    public decimal RailMarkupPercent { get; set; } = 0m;

    [MoneyPrecision]
    public decimal RailPerItemFee { get; set; } = 0m;
    public ServiceFeeType RailFeeType { get; set; } = ServiceFeeType.None;

    // --- Transfer Fees ---
    [MarkupPrecision]
    public decimal TransferMarkupPercent { get; set; } = 0m;

    [MoneyPrecision]
    public decimal TransferPerItemFee { get; set; } = 0m;
    public ServiceFeeType TransferFeeType { get; set; } = ServiceFeeType.None;

    // --- Activity Fees ---
    [MarkupPrecision]
    public decimal ActivityMarkupPercent { get; set; } = 0m;

    [MoneyPrecision]
    public decimal ActivityPerItemFee { get; set; } = 0m;
    public ServiceFeeType ActivityFeeType { get; set; } = ServiceFeeType.None;

    // --- Travel (Catch-All) Fees ---
    [MarkupPrecision]
    public decimal TravelMarkupPercent { get; set; } = 0m;

    [MoneyPrecision]
    public decimal TravelPerItemFee { get; set; } = 0m;
    public ServiceFeeType TravelFeeType { get; set; } = ServiceFeeType.None;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedAt { get; set;} = DateTime.UtcNow;
}
