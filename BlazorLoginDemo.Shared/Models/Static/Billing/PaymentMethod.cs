using System.ComponentModel.DataAnnotations;

namespace BlazorLoginDemo.Shared.Models.Static.Billing;
public enum PaymentMethod
{
    [Display(Name = "Invoice")]
    Invoice,

    [Display(Name = "PayPal")]
    PayPal,

    [Display(Name = "Stripe")]
    Stripe,

    [Display(Name = "Bank Transfer (AU)")]
    BankTransfer_AU,

    [Display(Name = "Bank Transfer (NZ)")]
    BankTransfer_NZ,

    [Display(Name = "Credit Card")]
    CreditCard,

    [Display(Name = "Apple Pay")]
    ApplePay,

    [Display(Name = "Google Pay")]
    GooglePay
}