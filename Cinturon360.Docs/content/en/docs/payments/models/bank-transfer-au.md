---
title: Bank Transfer (AU)
description: Australian bank transfer model, typically invoice-driven with local settlement details.
categories: [platform, payments]
tags: [billing, payment-models]
weight: 40
---

_Last updated: 24 January 2026_

## Summary

Bank Transfer (AU) is generally a postpaid model, aligned with invoice issuance and payment terms.

## TMC scope configuration

- account name
- BSB
- account number
- remittance instructions
- optional reference format for invoices

## Client scope configuration

- selected payment model: Bank Transfer (AU)
- payment terms (Net X)
- invoice delivery channel

## Operational behaviour

- charges accumulate → invoice issued → client pays via bank transfer
- reconciliation is typically manual or semi-automated (implementation dependent)
- delinquency rules apply if invoices remain unpaid beyond grace period

This model MUST define reconciliation evidence requirements once implemented.
