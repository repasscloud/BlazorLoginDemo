---
title: Delinquency and Locking
description: How overdue accounts are handled, how grace periods apply, and how accounts become locked.
categories: [platform, payments]
tags: [payment-flows]
weight: 60
---
## Delinquency definition

An account is delinquent when a payable amount remains outstanding beyond the due date and any configured grace period.

This can occur under:

- invoice-based models (unpaid invoices)
- card-based models (failed charges that remain unpaid)
- prepaid mode (insufficient funds, depending on contract and enforcement)

## Grace period

Grace period is the number of days after due date before enforcement applies.

See `payments/financials/grace-period.md`.

## Lock behaviour

When an account becomes locked:

- new charges MUST NOT be initiated
- booking approvals that would create payable amounts SHOULD be blocked (policy-dependent)
- administrative override may be available to TMC users (audited)

## Unlock behaviour

Unlock requires a deliberate action (manual intervention) and must be auditable:

- payment received and reconciled
- payment method replaced and successful payment captured
- authorised override by TMC finance/admin users

## Recommended lock states

- **Soft lock**: bookings allowed but no ticketing/payment capture permitted
- **Hard lock**: bookings and approvals blocked

Lock state selection is contract/policy dependent and must be configurable.
