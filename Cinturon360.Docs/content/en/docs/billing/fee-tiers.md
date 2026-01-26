---
title: Billing Fee Tiers
type: docs
weight: 50
---

## Purpose of fee tiers

Fee tiers represent **cost intent**, not exact processor pricing.

They decouple:

- business rules
- payment provider pricing
- regional differences

## Fee tiers

| Tier | Meaning |
|-----|--------|
| DomesticStandard | Low-cost domestic cards |
| DomesticHighCost | High-cost domestic cards |
| InternationalStandard | Standard international cards |
| InternationalHighCost | High-cost international cards |
| UnknownWorstCase | Fallback safety tier |

## High-cost brands

The following brands are always treated as high-cost:

- American Express
- JCB
- UnionPay

This applies whether they are domestic or international.

## Why tiers matter

Pricing logic works against tiers, not raw card data.

Later, each tier is mapped to a processor rate appropriate to the provider.
