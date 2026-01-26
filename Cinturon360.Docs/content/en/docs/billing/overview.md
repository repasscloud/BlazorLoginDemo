---
title: Overview
type: docs
weight: 20
---

## The core problem

When the platform charges a credit or debit card, the payment processor charges a fee on the **entire amount charged**.

If an invoice contains both:

- supplier or airline costs that must be passed through 1:1, and
- platform revenue (booking fees, service fees, access fees),

then the processor fee applies to **both**, unless explicitly compensated for.

If you do nothing, one of two bad things happens:

1. You silently subsidise supplier costs out of your margin
2. You under-collect margin and slowly bleed money

The billing model documented here exists to prevent that.

## Non-negotiable principles

1. **Airline and supplier fees are pass-through**
   - They are not revenue
   - They must be preserved 100%

2. **All platform revenue is explicit**
   - Booking fees
   - Change fees
   - Cancellation fees
   - Monthly platform fees
   - Percentage platform fees (capped or uncapped)

3. **Payment processor fees are never absorbed silently**
   - They are compensated for in pricing
   - Or explicitly accepted as a cost (by policy)

4. **Margins are calculated, not guessed**

5. **Domestic vs international is relative**
   - It depends on who is billing whom
   - There is no single “domestic country”

## Multi-tenant context

The same billing mechanics apply at every layer:

- Vendor billing a TMC
- TMC billing a Client
- Platform billing a Vendor

Only the **billing entity country** changes.

Everything else stays the same.
