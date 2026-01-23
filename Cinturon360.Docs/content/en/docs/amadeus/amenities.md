---
title: Amenities
description: Amadeus flight-offer amenities (amenityType + description) and normalization guidance
categories: [amadeus, external-api, amenities]
tags: [docs]
weight: 2
---


Two different fields matter:

- **`amenityType`**: a coarse category bucket (seat, baggage, lounge, etc.)
- **`description`**: a free-text label (not a stable enum) that varies by airline, program, and content source

## Typical shape

Amenities are commonly represented as an array of objects with (at least) these fields:

```json
{
  "amenityType": "BAGGAGE",
  "description": "CHECKED BAG 1 PC 23 KG EACH",
  "isChargeable": true
}
```

Treat `description` as *display text* and `amenityType` as the best "primary key" for grouping.

## AmenityType

| AmenityType | Meaning (best-effort) | Normalize to bucket | UI label suggestion | Notes |
|---|---|---|---|---|
| `BAGGAGE` | Carry-on / checked baggage inclusions and baggage ancillaries attached to the fare/segment. | `Baggage` | Baggage | Often the most concrete: pieces, kg/lb limits, etc. |
| `PRE_RESERVED_SEAT` | Pre-reserved / pre-assigned seat selection (included or purchasable). | `Seat` | Seat selection | May include "preferred zone", "premium seat", etc. |
| `MEAL` | Meal / snack / beverage inclusions or paid catering options. | `Meal` | Meals | Can be informational (available) vs included. |
| `BRANDED_FARES` | Fare-family bundle label/attributes ("Saver / Flex / Business Flex" style concept). | `FareBundle` | Fare bundle | Often describes conditions rather than a purchasable add-on. |
| `ENTERTAINMENT` | In-flight entertainment availability/inclusion. | `Entertainment` | Entertainment | Usually informational. |
| `TRAVEL_SERVICES` | Ancillary services beyond seat/meal/bags (priority boarding, fast track, etc.). | `Services` | Travel services | Broad bucket; keep raw description. |
| `LOUNGE` | Lounge access inclusion or purchasable lounge access. | `Lounge` | Lounge access | Sometimes time-limited (e.g., "3 HOURS"). |
| `UPGRADES` | Upgrade-related eligibility/offers (paid or points). | `Upgrades` | Upgrades | Rules are airline/program dependent. |

## Amenity Descriptions

These are **example** `description` strings you may see. They are not guaranteed to be complete or stable.

**Confidence** indicates how unambiguous the string is when interpreted without extra carrier context:
- **High**: meaning is clear from wording
- **Medium**: likely correct; could vary in nuance
- **Low**: ambiguous without airline/program details

### Buckets used below

- `Baggage`
- `Seat`
- `Meal`
- `Lounge`
- `Changes`
- `Refunds`
- `Loyalty`
- `Connectivity`
- `Power`
- `Penalties`
- `PassengerType`

### Description table

| Description | Meaning (best-effort) | Bucket | Confidence | Parsing hint |
|---|---|---:|:---:|---|
| 100 PERCENT KF MILES EARNED | Earn 100% of base miles in KrisFlyer (KF). | Loyalty | Medium | `%` + program code |
| 125 PERCENT KF MILES EARNED | Earn 125% of base miles in KrisFlyer (KF). | Loyalty | Medium | `%` + program code |
| 30 PERCENT MILES EARNED | Earn 30% of base miles (program unspecified). | Loyalty | Medium | `%` |
| 70 PERCENT MILES EARNED | Earn 70% of base miles (program unspecified). | Loyalty | Medium | `%` |
| 50 PCT QMILES ACCUMULATION | Earn 50% Qmiles (likely Qatar Privilege Club). | Loyalty | Medium | `PCT` + program word |
| MILEAGE ACCRUAL | Miles accrue (generic). | Loyalty | Medium | keyword |
| 1PC MAX 15LB 7KG 115LCM | 1 piece allowance with max weight and max dimensions. | Baggage | Medium | `1PC` + `LB/KG` + `LCM` |
| CABIN BAG 1 PIECE 7 KG | Cabin baggage: 1 piece up to 7kg. | Baggage | High | `CABIN BAG` |
| CABIN BAG 7KG X 1 | Cabin baggage: 1×7kg. | Baggage | High | `7KG` + `X 1` |
| CARRY ON HAND BAGGAGE | Carry-on/hand baggage permitted/included. | Baggage | High | keyword |
| CHECKED BAG | Checked bag included/available. | Baggage | High | keyword |
| CHECKED BAG 1 PC 23 KG EACH | Checked bag allowance: 1 piece, 23kg each. | Baggage | High | `PC` + `KG EACH` |
| 40KG BAGGAGE ALLOWANCE | Baggage allowance: 40kg (usually checked). | Baggage | High | `KG` |
| 50KG BAGGAGE ALLOWANCE | Baggage allowance: 50kg (usually checked). | Baggage | High | `KG` |
| PRE PAID BAGGAGE | Baggage pre-paid (ancillary). | Baggage | Medium | keyword |
| BASIC SEAT | Basic seating product. | Seat | Medium | keyword |
| STANDARD SEATING | Standard seating included. | Seat | High | keyword |
| PREMIUM SEAT | Premium/preferred seat product. | Seat | Medium | keyword |
| SEAT ASSIGNMENT | Seat assignment included/available. | Seat | High | keyword |
| PRE SEAT ASSIGNMENT | Pre seat assignment included/available. | Seat | High | keyword |
| PRE RESERVED SEAT ASSIGNMENT | Seat can be assigned in advance (pre-reserved). | Seat | High | keyword |
| SEAT SELECTION FORWARD ZONE | Seat selection in forward zone/section. | Seat | Medium | `FORWARD ZONE` |
| SEAT SELECTION STANDARD ZONE | Standard zone seat selection. | Seat | Medium | `STANDARD ZONE` |
| MEAL | Meal included/available. | Meal | High | keyword |
| SNACK | Snack included/available. | Meal | Medium | keyword |
| BEVERAGE | Beverage included/available. | Meal | Medium | keyword |
| COMPLIMENTARY BEVERAGES | Complimentary beverages included. | Meal | High | keyword |
| MEAL BEVERAGE | Meal + beverage included/available. | Meal | High | keywords |
| MEAL OR SNACK | Meal or snack depending on flight/service. | Meal | Medium | keywords |
| MEAL VOUCHER | Meal voucher provided. | Meal | Medium | keyword |
| 3 HOURS LOUNGE ACCESS | Lounge access for 3 hours. | Lounge | High | `HOURS` |
| INFLIGHT WIFI | In-flight Wi-Fi included/available. | Connectivity | Medium | keyword |
| USB POWER | USB power available at seat. | Power | High | keyword |
| BOOKING CHANGE | Changes permitted (rules apply). | Changes | High | keyword |
| CHANGEABLE TICKET | Ticket is changeable. | Changes | High | keyword |
| CHANGE BEFORE DEPARTURE | Changes allowed before departure. | Changes | High | keyword |
| CHANGE AFTER DEPARTURE | Changes allowed after departure. | Changes | High | keyword |
| CHANGE FEE | Change fee applies. | Changes | High | keyword |
| CANCELLATION | Cancellation policy applies (may or may not be permitted). | Refunds | Medium | keyword |
| REFUNDABLE TICKET | Ticket is refundable (rules apply). | Refunds | High | keyword |
| REFUND BEFORE DEPARTURE | Refund permitted before departure (rules apply). | Refunds | High | keyword |
| NO SHOW | No-show policy/penalty applies. | Penalties | Medium | keyword |
| CHILD DISCOUNT | Child passenger discount/conditions. | PassengerType | Medium | keyword |
| INFANT DISCOUNT | Infant passenger discount/conditions. | PassengerType | Medium | keyword |
| UPGRADE ELIGIBILITY | Eligible for upgrades (method depends). | Upgrades | Medium | keyword |
| UPGRADE WITH MILES PWM | Upgrade using miles; `PWM` unclear without extra context. | Upgrades | Low | keep raw |

## Normalization strategy (recommended)

1. **Keep the raw strings**:
   - `amenityTypeRaw`
   - `descriptionRaw`

2. Normalize into a small set of **buckets** used by your UI and business rules.

3. Extract **structured facts** when the description obviously contains them (e.g., baggage pieces/weight, lounge hours, mileage %).

4. Handle unknown strings safely:
   - Display them as-is
   - Log for later mapping improvements

## Example normalization code (C#)

Minimal "good enough" normalizer: bucket + optional parsed values.

```csharp
using System.Text.RegularExpressions;

public enum AmenityBucket
{
    Unknown,
    Baggage,
    Seat,
    Meal,
    Lounge,
    Changes,
    Refunds,
    Loyalty,
    Connectivity,
    Power,
    Penalties,
    PassengerType,
    Upgrades,
    FareBundle,
    Entertainment,
    Services
}

public sealed record AmenityNormalized(
    AmenityBucket Bucket,
    string Confidence,            // "High" | "Medium" | "Low"
    int? Pieces = null,
    decimal? WeightKg = null,
    decimal? WeightLb = null,
    int? LoungeHours = null,
    int? MilesPercent = null,
    string? LoyaltyProgram = null
);

public static class AmenityNormalizer
{
    private static readonly Regex RxMilesPct =
        new(@"(?i)(?<pct>\d{1,3})\s*(PERCENT|PCT).*?(?<prog>KF|QMILES)?", RegexOptions.Compiled);

    private static readonly Regex RxCheckedBagPiecesKg =
        new(@"(?i)CHECKED\s+BAG.*?(?<pc>\d+)\s*(PC|PIECE).*?(?<kg>\d+(\.\d+)?)\s*KG", RegexOptions.Compiled);

    private static readonly Regex RxCabinBagPiecesKg =
        new(@"(?i)CABIN\s+BAG.*?(?<pc>\d+)\s*(PC|PIECE|X).*?(?<kg>\d+(\.\d+)?)\s*KG", RegexOptions.Compiled);

    private static readonly Regex RxLoungeHours =
        new(@"(?i)(?<h>\d+)\s*HOURS?\s+LOUNGE\s+ACCESS", RegexOptions.Compiled);

    public static AmenityNormalized Normalize(string? description, string? amenityType)
    {
        var d = (description ?? string.Empty).Trim();

        // 1) Prefer amenityType when present (coarse but stable)
        var bucketFromType = amenityType?.Trim().ToUpperInvariant() switch
        {
            "BAGGAGE" => AmenityBucket.Baggage,
            "PRE_RESERVED_SEAT" => AmenityBucket.Seat,
            "MEAL" => AmenityBucket.Meal,
            "LOUNGE" => AmenityBucket.Lounge,
            "UPGRADES" => AmenityBucket.Upgrades,
            "BRANDED_FARES" => AmenityBucket.FareBundle,
            "ENTERTAINMENT" => AmenityBucket.Entertainment,
            "TRAVEL_SERVICES" => AmenityBucket.Services,
            _ => AmenityBucket.Unknown
        };

        // 2) Parse obvious structured facts from description
        if (RxLoungeHours.Match(d) is { Success: true } mL)
        {
            return new(AmenityBucket.Lounge, "High", LoungeHours: int.Parse(mL.Groups["h"].Value));
        }

        if (RxCheckedBagPiecesKg.Match(d) is { Success: true } mB)
        {
            return new(AmenityBucket.Baggage, "High",
                Pieces: int.Parse(mB.Groups["pc"].Value),
                WeightKg: decimal.Parse(mB.Groups["kg"].Value));
        }

        if (RxCabinBagPiecesKg.Match(d) is { Success: true } mC)
        {
            // Cabin bag is still "Baggage", but you might store a sub-type if you want.
            return new(AmenityBucket.Baggage, "High",
                Pieces: int.Parse(mC.Groups["pc"].Value),
                WeightKg: decimal.Parse(mC.Groups["kg"].Value));
        }

        if (RxMilesPct.Match(d) is { Success: true } mM)
        {
            var pct = int.Parse(mM.Groups["pct"].Value);
            var prog = mM.Groups["prog"].Success ? mM.Groups["prog"].Value.ToUpperInvariant() : null;

            return new(AmenityBucket.Loyalty, prog is null ? "Medium" : "Medium",
                MilesPercent: pct, LoyaltyProgram: prog);
        }

        // 3) Keyword fallback (for when amenityType is missing/unknown)
        var u = d.ToUpperInvariant();
        if (u.Contains("WIFI")) return new(AmenityBucket.Connectivity, "Medium");
        if (u.Contains("USB POWER")) return new(AmenityBucket.Power, "High");
        if (u.Contains("NO SHOW")) return new(AmenityBucket.Penalties, "Medium");
        if (u.Contains("REFUND")) return new(AmenityBucket.Refunds, "High");
        if (u.Contains("CHANGE")) return new(AmenityBucket.Changes, "High");
        if (u.Contains("MEAL") || u.Contains("SNACK") || u.Contains("BEVERAGE")) return new(AmenityBucket.Meal, "Medium");
        if (u.Contains("SEAT")) return new(AmenityBucket.Seat, "Medium");
        if (u.Contains("BAG") || u.Contains("BAGGAGE")) return new(AmenityBucket.Baggage, "Medium");

        // 4) If amenityType gave us something usable, keep it
        if (bucketFromType != AmenityBucket.Unknown)
            return new(bucketFromType, "Medium");

        return new(AmenityBucket.Unknown, "Low");
    }
}
```

### Notes

- This approach is intentionally conservative: it extracts only facts that are strongly implied by the string.
- Keep improving the regexes as you encounter new carrier strings.
- Always keep the **raw** description alongside your normalized representation.
