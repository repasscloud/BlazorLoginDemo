---
title: Payments
description: Payment models, onboarding flows, legal authorizations, and financial controls across Vendor → TMC → Client relationships.
categories: [platform, payments]
tags: [billing, stripe, invoice, legal, financials]
weight: 10
---

_Last updated: 24 January 2026_

## Audience and intent

This documentation is written for:

- Cinturon360 Vendor delivery and support teams
- TMC implementation and operations teams
- Client onboarding and Accounts teams (as referenced by TMC support)
- Engineering teams implementing payment features and integrations

## Core concepts

### Entities

- **Vendor**: The company selling and operating the Cinturon360 platform.
- **TMC**: Travel Management Company selling travel services to Clients via Cinturon360.
- **Client**: End-user company purchasing travel services from the TMC (example: *Big Company A Pty Ltd (BCA)*).
- **Traveller / Requester**: End-user initiating bookings under Client policy.
- **Approver**: Person or workflow that approves bookings and/or transactions.

### Payment configuration scope

Payments are configured at multiple levels:

- **Vendor scope**: platform-wide capabilities, defaults, enforcement (e.g., supported payment models, default legal templates).
- **TMC scope**: which payment methods are enabled on the TMC account, settlement configuration, tax rules, and operational preferences.
- **Client scope**: the contractually selected payment model(s), billing terms, prepaid/postpaid behaviour, and account financial controls.

### Billability

A Client account is **billable** when:

- a payment model is selected for the Client–TMC relationship, **and**
- the configuration required by that model is complete (e.g., a valid payment method on file for Stripe / Credit Card), **and**
- the account is not in a locked or delinquent state, **and**
- the transaction is permitted under thresholds / policy controls.

Billability is evaluated at the time of charge initiation, not only at onboarding time.

## Documentation structure

- **Payment models**: what each model means and its operational requirements.
- **Onboarding**: how payment methods are set up (especially Stripe).
- **Flows**: charge lifecycle, refunds, credits, disputes, and locking.
- **Legal**: default platform-hosted authorizations and custom agreements.
- **Financials**: thresholds, prepaid/postpaid, grace periods, fees.
- **Support**: operational checklists and troubleshooting.

## Quick links

- Payment models: `payments/models/`
- Stripe onboarding: `payments/onboarding/stripe-client-onboarding.md`
- Charge lifecycle: `payments/flows/charge-lifecycle.md`
- Legal templates and custom agreements: `payments/legal/`
- Financial controls: `payments/financials/`
