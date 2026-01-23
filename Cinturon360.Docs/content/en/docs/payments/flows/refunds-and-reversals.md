---
title: Refunds and Reversals
description: How refunds, reversals, and supplier credits are processed and evidenced.
categories: [platform, payments]
tags: [payment-flows]
weight: 30
---

## Refund sources

Refunds may originate from:

- supplier refunds (airline/hotel)
- partial reversals due to changes
- over-collection corrections
- service fee adjustments (contract dependent)

## Refund destinations

Refunds may be applied as:

- refund to original payment method (card refund via processor)
- credit to account (ledger credit)
- prepaid balance top-up (if configured)
- offset against future invoices (invoice model)

## Evidence requirements

Refund processing MUST capture:

- source charge reference
- refund amount and currency
- reason code / narrative
- supplier reference (if applicable)
- processor refund reference (if card)
- who initiated the refund and when
