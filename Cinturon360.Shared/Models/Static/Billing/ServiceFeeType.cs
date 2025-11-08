using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Static.Billing;
public enum ServiceFeeType
{
    [Display(Name = "None")]
    None = 0,               // No markup or per-item fee

    [Display(Name = "Markup Only")]
    MarkupOnly = 1,         // Only percentage markup

    [Display(Name = "Per Item Fee Only")]
    PerItemFeeOnly = 2,     // Only per-item fixed fee

    [Display(Name = "Markup & Per Item Fee")]
    MarkupAndPerItemFee = 3 // Both markup and per-item fee
}
