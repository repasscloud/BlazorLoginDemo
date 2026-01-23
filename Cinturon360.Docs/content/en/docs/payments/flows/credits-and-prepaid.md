---
title: Credits and Prepaid Balance
description: How credits and prepaid balances are applied, offset, and reported.
categories: [platform, payments]
tags: [payment-flows]
weight: 40
---

_Last updated: 24 January 2026_

## Definitions

- **Credit**: a ledger entry that reduces the amount payable by the Client.
- **Prepaid balance**: an amount held on the account that is consumed instead of charging a payment method.
- **Prepaid mode**: the relationship can only spend available prepaid balance.
- **Postpaid mode**: the relationship charges payment method or invoices as amounts become payable.

## Prepaid vs Postpaid rules

### Prepaid

- charges MUST consume prepaid balance first
- if balance is insufficient, the charge MUST NOT be initiated (unless contract allows fallback)
- approval may still be blocked by thresholds and capacity rules

### Postpaid

- charges are initiated via the selected payment model (Stripe/invoice/etc.)
- credits may be applied to reduce amounts due
- prepaid balance (if present) may offset charges if configured

## Offset order (recommended default)

When an account has multiple offset mechanisms, a deterministic order is required. Recommended order:

1. Apply account credits (ledger credit entries) where applicable
2. Consume prepaid balance (if configured to offset)
3. Charge via payment model (Stripe/card) or invoice

This order MUST be consistent and auditable.

## Reporting

Prepaid and credit must be reportable to:

- Vendor and TMC finance teams
- Client accounts teams (where appropriate)
- support operations for delinquency analysis

Reporting must show:

- opening balance
- credits applied
- charges consumed
- closing balance
