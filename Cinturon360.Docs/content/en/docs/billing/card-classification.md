---
title: Card Classification
type: docs
weight: 40
---

## Card brand

The platform recognises the following brands:

- Visa
- MasterCard
- American Express
- JCB
- UnionPay
- Discover
- Diners Club

Unknown or unsupported brands are treated as **worst case**.

## Card issuing country

The card issuing country is stored as a two-letter ISO-2 code (e.g. AU, US, GB, JP).

This value is used to determine whether a card is **domestic or international**, relative to the billing entity.

## Card funding type

Funding types include:

- Credit
- Debit
- Prepaid
- Unknown

Funding type is currently informational, but retained for:

- reporting
- future optimisation
- alternative payment providers

## Why classification is provider-agnostic

Card classification is stored in **domain enums**, not Stripe-specific strings.

This allows the same logic to be reused for:

- Stripe
- PayPal
- Adyen
- Future gateways

Only the adapter layer changes.
