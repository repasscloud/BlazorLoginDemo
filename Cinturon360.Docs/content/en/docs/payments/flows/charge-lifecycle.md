---
title: Charge Lifecycle
description: How charges are created, approved, invoiced or captured, and reconciled across payment models.
categories: [platform, payments]
tags: [payment-flows]
weight: 20
---

_Last updated: 24 January 2026_

## Lifecycle overview

A “charge” in Cinturon360 is the financial representation of a payable amount arising from:

- supplier costs (air/hotel/car/rail/etc.)
- TMC service fees
- change/cancellation penalties
- taxes and government charges
- pass-through surcharges

Charges may be created at multiple points in the booking lifecycle.

## Trigger points

Common trigger points include:

- booking creation (quote stage)
- approval confirmation (approved state)
- ticketing (air)
- supplier confirmation (hotel/car)
- post-booking modifications
- cancellations and no-shows
- post-trip adjustments and reconciliations

## Approval gating

Charges may only be captured/invoiced when they qualify as **Approved Travel Charges**.

Approval sources:

- explicit approval workflow in the platform
- auto-approval by contract/policy (threshold dependent)
- manual override by authorised users (audited)

## Stripe charge lifecycle (typical)

1. booking is approved
2. charge intent is created with metadata (booking id, service type, client relationship id)
3. platform initiates off-session payment via Stripe
4. Stripe returns success/failure
5. platform records outcome and attaches evidence to the booking and finance ledger

## Invoice charge lifecycle (typical)

1. booking is approved
2. charge is recorded in ledger as “invoicable”
3. invoice run generates invoice lines for the billing period
4. invoice is issued and delivered
5. payments are reconciled and ledger is marked settled

## State machine (conceptual)

```
Draft → PendingApproval → Approved → Billable → Charged/Settled
                           |             |
                           |             └─ Failed → Retry/PastDue → Locked (if overdue)
                           |
                           └─ Cancelled/Expired (no billing)
```

Each model maps the “Charged/Settled” state differently (capture vs invoice settlement).
