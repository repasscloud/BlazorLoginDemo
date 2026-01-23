---
title: Payments Architecture
description: End-to-end payment architecture, including configuration inheritance, billability, charge initiation, and settlement responsibilities.
categories: [platform, payments]
tags: [architecture, billing, multi-tenant]
weight: 20
---

## Summary

Cinturon360 supports multiple payment models to accommodate TMC and Client contractual arrangements. The platform must support:

- **prepaid** and **postpaid** relationships
- **stored payment credentials** (card / Stripe-based) and **invoice** relationships
- **regional bank transfer** options (AU and NZ)
- **alternative wallets** (Apple Pay, Google Pay) when enabled by the TMC’s payment processor configuration
- a **credit / prepaid balance** mechanism that can offset or replace charging a payment method

## Configuration inheritance

Configuration is evaluated in a hierarchy:

1. Vendor-level platform defaults (capabilities, default legal templates, global enforcement rules)
2. TMC-level enablement and processor configuration (e.g., Stripe account, PayPal enablement, bank transfer details)
3. Client-level contractual selection and operational configuration (e.g., payment model, terms, prepaid/postpaid)

Rules:

- TMC can only offer payment models that are enabled and configured at TMC scope.
- Client can only select payment models that are (a) offered by the TMC and (b) permitted by the contract.
- For each payment model, **completeness** must be satisfied before billing is enabled.

## Responsibility model (merchant of record and settlement)

For each charge, the system must clearly determine:

- **Merchant of Record (MoR)**: whose name appears for card charges and who contracts for payment processing.
- **Settlement party**: who receives funds from the payment processor.
- **Invoice issuer**: who issues invoices when invoice-based models are used.

These values may vary by implementation and contract. Placeholders are used in Client-facing terms where required.

## Billability evaluation (high-level)

Billability checks are applied when a charge is initiated:

- Is the account locked or delinquent?
- Is the payment model allowed for this Client–TMC relationship?
- Does the model have required setup completed?
- Are threshold/limits satisfied?
- Is there sufficient prepaid balance (for prepaid mode)?
- Is the booking/charge approved or auto-approved per contract and policy settings?

Billability is not a single switch; it is the result of multiple checks.

## Directory structure

```
content/en/docs/payments/
├── _index.md
├── architecture.md
├── models/
│   ├── _index.md
│   ├── invoice.md
│   ├── stripe.md
│   ├── paypal.md
│   ├── bank-transfer-au.md
│   ├── bank-transfer-nz.md
│   ├── credit-card.md
│   ├── apple-pay.md
│   └── google-pay.md
├── onboarding/
│   ├── _index.md
│   ├── stripe-client-onboarding.md
│   └── stripe-reonboarding-and-replacement.md
├── flows/
│   ├── _index.md
│   ├── charge-lifecycle.md
│   ├── refunds-and-reversals.md
│   ├── credits-and-prepaid.md
│   ├── disputes-and-chargebacks.md
│   └── delinquency-and-locking.md
├── legal/
│   ├── _index.md
│   ├── default-terms.md
│   ├── custom-agreements.md
│   ├── standing-payment-authorization.md
│   └── placeholders.md
├── financials/
│   ├── _index.md
│   ├── access-fees.md
│   ├── thresholds.md
│   ├── tax-and-invoicing.md
│   ├── minimum-monthly-spend.md
│   ├── prepaid-balance.md
│   └── grace-period.md
├── terms/
│   ├── _index.md
│   └── payment-terms.md
├── support/
│   ├── _index.md
│   ├── onboarding-checklist.md
│   └── troubleshooting.md
└── placeholders/
    ├── discounts.md
    ├── pnr-service-fees.md
    ├── late-fees.md
    └── capacity-limits.md
```
