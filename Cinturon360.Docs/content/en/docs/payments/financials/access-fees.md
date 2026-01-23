---
title: Access Fees
description: Access fee configuration (PAYG, monthly, quarterly, bi-annual, annual) and how fees are offset against prepaid balance where configured.
categories: [platform, payments]
tags: [financials, payments]
weight: 20
---

## Fields

- **Access Fee**: the monetary amount charged for platform access.
- **Access Fee Scope**: the billing interval that defines when the access fee is due.
  - PAYG
  - Monthly
  - Quarterly
  - Bi-Annual
  - Annual

## Charging behaviour

Access fees may be charged:

- to invoice (invoice models)
- to stored payment method (card/Stripe) if contract permits
- against prepaid balance (if configured)

## Prepaid offsets

If prepaid balance offsets are enabled, the offset order must be applied consistently (see `payments/flows/credits-and-prepaid.md`).

## Reporting

Access fees must be reportable by:

- account
- scope interval
- outstanding vs settled
