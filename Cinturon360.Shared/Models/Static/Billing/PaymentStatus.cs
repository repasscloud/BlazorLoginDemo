using System.ComponentModel.DataAnnotations;

namespace Cinturon360.Shared.Models.Static.Billing;
public enum PaymentStatus
{
    [Display(Name = "Pending")]
    Pending = 0,

    [Display(Name = "Processing")]
    Processing = 1,

    [Display(Name = "Authorized")]
    Authorized = 2,

    [Display(Name = "Partially Paid")]
    PartiallyPaid = 3,

    [Display(Name = "Paid")]
    Paid = 4,

    [Display(Name = "Failed")]
    Failed = 5,

    [Display(Name = "Overdue")]
    Overdue = 6,

    [Display(Name = "Disputed")]
    Disputed = 7,

    [Display(Name = "Refunded")]
    Refunded = 8,

    [Display(Name = "Partially Refunded")]
    PartiallyRefunded = 9,

    [Display(Name = "Cancelled")]
    Cancelled = 10,

    [Display(Name = "Written Off")]
    WrittenOff = 11,

    [Display(Name = "Chargeback")]
    Chargeback = 12,

    [Display(Name = "Voided")]
    Voided = 13,

    [Display(Name = "Scheduled")]
    Scheduled = 14,

    [Display(Name = "On Hold")]
    OnHold = 15
}
