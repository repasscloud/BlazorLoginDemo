using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Static.Billing;
public enum BillingFrequency
{
    [Display(Name = "Pay As You Go (PAYG)")]
    PayAsYouGo = 0,

    [Display(Name = "Monthly")]
    Monthly = 30,

    [Display(Name = "Quarterly")]
    Quarterly = 90,

    [Display(Name = "Bi-Annually")]
    BiAnnually = 182,

    [Display(Name = "Annually")]
    Annually = 365
}
