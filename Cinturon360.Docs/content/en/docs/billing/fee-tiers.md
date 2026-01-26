---
title: Billing Fee Tiers
type: docs
weight: 50
---

## Purpose of fee tiers

Fee tiers represent **cost intent**, not exact processor pricing.

They deliberately decouple:

- business rules
- payment provider pricing (Stripe, etc.)
- regional and regulatory differences

The tier is resolved once, early, and reused consistently across billing, reporting, and reconciliation.

## Tier definitions and indicative percentages

> Percentages shown are **indicative Stripe-style AU rates**, not contractual guarantees.

| Tier | Indicative % | Meaning |
|-----|-------------|--------|
| `Unset` | 0.00% | Invalid / placeholder only |
| `UnknownWorstCase` | 3.50% | Defensive fallback when card data is incomplete |
| `DomesticStandard` | 1.75% | AU-issued Visa / Mastercard |
| `DomesticHighCost` | 3.50% | AU-issued Amex / Diners |
| `InternationalStandard` | 2.90% | Non-AU Visa / Mastercard |
| `InternationalHighCost` | 3.50% | Non-AU Amex / Diners |

## High-cost brands

The following brands are always treated as **high-cost**, regardless of country:

- American Express
- Diners Club
- JCB
- UnionPay

They map to either `DomesticHighCost` or `InternationalHighCost`.

## Why 3.50% is reused

Stripe effectively **caps high-cost brands** around this level in Australia.

While prepaid, commercial, and some cross-border cases vary internally, **3.50% is the correct “never under-recover” ceiling** when pricing defensively.

This is why the `UnknownWorstCase` tier exists and intentionally matches high-cost pricing.

## What this means for the model

- `CardFunding` **does not change the tier** (by design)
- Funding differences are absorbed inside Stripe’s blended pricing
- Tier resolution remains simple, deterministic, and auditable

The enum is intentionally **sound and minimal**.

## Next steps (optional extensions)

If required, pricing can be extended without changing tier resolution:

- Add **flat fee cents per tier** (e.g. +$0.30)
- Split **platform fee vs processor fee** explicitly
- Apply **caps or negotiated overrides** per TMC

All of these layer cleanly *after* tier resolution.
