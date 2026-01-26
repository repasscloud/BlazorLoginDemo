---
title: Multi-tenant Domestic vs International
type: docs
weight: 90
---

## Definition

A card is domestic if:

CardCountry == BillingEntityCountry

Otherwise, it is international.

## Examples

Vendor AU, TMC GB, Client card US:

- TMC billing Client
- Billing entity country = GB
- Card country = US
- Result: International

## Why this matters

Hard-coding a country (e.g. AU) breaks multi-tenant billing.

Domestic is always relative.
