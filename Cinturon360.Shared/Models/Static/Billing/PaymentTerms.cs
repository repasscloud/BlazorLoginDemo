using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Static.Billing;
public enum PaymentTerms
{
    [Display(Name = "Net 0 Days")]
    Net0 = 0,

    [Display(Name = "Net 1 Day")]
    Net1 = 1,

    [Display(Name = "Net 7 Days")]
    Net7 = 7,

    [Display(Name = "Net 14 Days")]
    Net14 = 14,

    [Display(Name = "Net 21 Days")]
    Net21 = 21,

    [Display(Name = "Net 30 Days")]
    Net30 = 30,
    
    [Display(Name = "Net 60 Days")]
    Net60 = 60,

    [Display(Name = "Net 90 Days")]
    Net90 = 90
}
