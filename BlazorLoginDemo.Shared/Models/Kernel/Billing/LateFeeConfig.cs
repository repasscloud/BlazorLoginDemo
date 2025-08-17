using System.ComponentModel.DataAnnotations;
using BlazorLoginDemo.Shared.Models.Static.Billing;
using BlazorLoginDemo.Shared.Validation;
using NanoidDotNet;

namespace BlazorLoginDemo.Shared.Models.Kernel.Billing;

public class LateFeeConfig
{
    [Key]
    [MaxLength(12)]
    public string Id { get; set; } = Nanoid.Generate(Nanoid.Alphabets.UppercaseLettersAndDigits, 12);

    [MaxLength(14)]
    [Required]
    public required string LicenseAgreementId { get; set;}
    public int GracePeriodDays { get; set; } = 0;
    public bool UseFixedAmount { get; set; } = false;

    [MoneyPrecision]
    public decimal FixedAmount { get; set; } = 0m;

    [TaxPrecision]
    public decimal PercentOfInvoice { get; set; } = 0m;
    public RecurringLateFeeOption RecurringOption { get; set; } = RecurringLateFeeOption.None;

    [MoneyPrecision]
    public decimal MaxLateFeeCap { get; set; } = 0m;
}
