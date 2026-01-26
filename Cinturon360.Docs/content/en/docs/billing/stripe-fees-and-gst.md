---
title: Stripe Fees and GST
type: docs
weight: 60
---

## Stripe processing fees

Stripe charges a processing fee on the **entire amount charged to the card**.

The effective rate depends on:

- card brand
- issuing country
- your Stripe pricing agreement

In test mode, AMEX often reflects worst-case behaviour.

## GST on Stripe fees (Australia)

Stripe charges **10% GST on its own service fee**.

Important clarifications:

- This is NOT customer GST
- This is NOT Stripe Tax (the product)
- This is GST on Stripe’s service
- It is typically claimable as an input tax credit

Stripe deductions therefore look like:

- Stripe processing fee
- GST on Stripe processing fee

## Why this matters

If you ignore GST on Stripe fees:

- your net margin will be wrong
- your accounting reconciliation will not match Stripe

The platform model explicitly compensates for this.
