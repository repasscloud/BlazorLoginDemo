---
title: Payment Models
description: Definitions, setup requirements, and operational behaviour of each payment model.
categories: [platform, payments]
tags: [billing, payment-models]
weight: 10
---

_Last updated: 24 January 2026_

## Model list

Supported payment models currently documented:

- Invoice
- PayPal
- Stripe
- Bank Transfer (AU)
- Bank Transfer (NZ)
- Credit Card
- Apple Pay
- Google Pay

More models may be added. When a new model is introduced it MUST define:

- configuration requirements (TMC scope and Client scope)
- billability checks
- charge lifecycle behaviour
- refund/credit behaviour
- legal and audit requirements
- support/troubleshooting playbooks

## Model selection rules

- A Client–TMC relationship MUST have a selected model before any charges can be initiated.
- The selected model MUST be contractually permitted.
- Switching models MUST preserve auditability of prior authorizations and payment evidence.
