namespace Cinturon360.Shared.Models.Static.Billing;
public enum BillingFeeTier
{
    Unset = 0,                    // 0.00% (should never be charged)
    UnknownWorstCase = 1,         // 3.50% (defensive ceiling)
    DomesticStandard = 2,         // 1.75% (AU Visa/MC equivalent)
    DomesticHighCost = 3,         // 3.50% (AU Amex / Diners equivalent)
    InternationalStandard = 4,    // 2.90% (Non-AU Visa/MC equivalent)
    InternationalHighCost = 5     // 3.50% (Non-AU Amex / Diners equivalent)
}