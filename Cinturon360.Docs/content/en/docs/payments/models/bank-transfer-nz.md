---
title: Bank Transfer (NZ)
description: New Zealand bank transfer model, typically invoice-driven with NZ settlement details.
categories: [platform, payments]
tags: [billing, payment-models]
weight: 50
---
## Summary

Bank Transfer (NZ) is generally a postpaid model, aligned with invoice issuance and payment terms.

## TMC scope configuration

- account name
- bank account number (NZ format)
- SWIFT (if required)
- remittance instructions
- optional reference format for invoices

## Client scope configuration

- selected payment model: Bank Transfer (NZ)
- payment terms (Net X)
- invoice delivery channel

## Operational behaviour

- charges accumulate → invoice issued → client pays via bank transfer
- delinquency rules apply if invoices remain unpaid beyond grace period
