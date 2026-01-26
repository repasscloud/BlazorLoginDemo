---
title: Developer Notes
type: docs
weight: 100
---

## Design intent

- Rules are pure and deterministic
- Adapters fetch data from providers
- Models store stable classification
- Calculations happen before charging

## Extending to other providers

To add a provider:

1. Map provider card data to enums
2. Resolve BillingFeeTier
3. Map tier to processor rate
4. Reuse existing formulas

## Anti-patterns

- Hiding margin in airline fees
- Ignoring processor GST
- Treating domestic as a global concept
- Letting users edit cost profiles
