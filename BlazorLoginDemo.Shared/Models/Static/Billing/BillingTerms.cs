using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Models.Static.Billing;
public enum PaymentTerms
{
    [Display(Name = "Net 0 Days")]
    Net0 = 0,

    [Display(Name = "Net 30 Days")]
    Net30 = 30,
    
    [Display(Name = "Net 60 Days")]
    Net60 = 60,

    [Display(Name = "Net 90 Days")]
    Net90 = 90
}
