using static Cinturon360.Shared.Models.Kernel.Billing.LicenseAgreementUnified;

namespace Cinturon360.Shared.Models.Kernel.Billing;

public static class BillingProfileRulesHelper
{
    // ----------------------------
    // Domestic vs International
    // ----------------------------

    public static bool IsDomestic(string cardCountry, string billingEntityCountry)
        => !string.IsNullOrWhiteSpace(cardCountry)
        && !string.IsNullOrWhiteSpace(billingEntityCountry)
        && cardCountry.Equals(billingEntityCountry, StringComparison.OrdinalIgnoreCase);


    // ----------------------------
    // Brand groupings (cost intent)
    // ----------------------------

    public static bool IsHighCostBrand(CardBrand brand)
        => brand is
            CardBrand.AmericanExpress or
            CardBrand.JCB or
            CardBrand.UnionPay;

    public static bool IsStandardBrand(CardBrand brand)
        => brand is
            CardBrand.Visa or
            CardBrand.MasterCard or
            CardBrand.Discover or
            CardBrand.DinersClub;


    // ----------------------------
    // Composite risk flags
    // ----------------------------

    public static bool IsHighCost(
        CardBrand brand,
        string cardCountry,
        string billingEntityCountry)
        => IsHighCostBrand(brand)
        || !IsDomestic(cardCountry, billingEntityCountry);


    // ----------------------------
    // Fee tier resolution (authoritative)
    // ----------------------------

    public static BillingFeeTier ResolveFeeTier(
        CardBrand brand,
        string cardCountry,
        string billingEntityCountry)
    {
        // Hard fallbacks
        if (brand is CardBrand.Unset or CardBrand.Unknown or CardBrand.Other)
            return BillingFeeTier.UnknownWorstCase;

        bool isDomestic = IsDomestic(cardCountry, billingEntityCountry);
        bool isHighCostBrand = IsHighCostBrand(brand);

        // Domestic cards (relative to billing entity)
        if (isDomestic)
        {
            return isHighCostBrand
                ? BillingFeeTier.DomesticHighCost
                : BillingFeeTier.DomesticStandard;
        }

        // International cards
        return isHighCostBrand
            ? BillingFeeTier.InternationalHighCost
            : BillingFeeTier.InternationalStandard;
    }
}
