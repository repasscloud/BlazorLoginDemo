---
title: Financials
description: Commercial and enforcement controls applied to TMC and Client accounts.
categories: [platform, payments]
tags: [financials, payments]
weight: 10
---

## Summary

Financial configuration controls:

- how access fees are charged
- how spend thresholds are applied and reset
- what tax rates are applied to invoices and fees
- minimum monthly spend reporting and enforcement
- prepaid balance and credit application rules
- delinquency grace periods and locking

Financials are configured per account scope and applied to descendant relationships where appropriate.

## Key rules

- Financial controls MUST be deterministic and auditable.
- Threshold resets MUST be based on a configured scope (PAYG, monthly, quarterly, bi-annual, annual).
- Locking behaviour MUST prevent further charge initiation when enforcement is triggered.
