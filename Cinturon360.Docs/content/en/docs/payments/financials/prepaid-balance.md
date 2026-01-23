---
title: Prepaid Balance
description: How prepaid balances are applied to access fees and travel charges, and how prepaid-only accounts operate.
categories: [platform, payments]
tags: [financials, payments]
weight: 60
---
## Field

- **Prepaid Balance**: amount available for consumption by charges.

## Modes

### Prepaid-only

Prepaid-only accounts:

- can only pay for travel charges using available prepaid balance
- do not charge a stored payment method (unless explicit fallback is configured contractually)
- are blocked from approving/issuing charges that exceed balance

### Hybrid (prepaid offset)

Hybrid accounts:

- may have prepaid balance that offsets charges first
- use Stripe/card/invoice for remaining amounts when prepaid is insufficient (if enabled)

The offset order must be deterministic.
