---
title: Troubleshooting
description: Common payment setup and billing issues, symptoms, and resolution paths.
categories: [platform, payments]
tags: [troubleshooting, support]
weight: 30
---

_Last updated: 24 January 2026_

## Common issues

### Stripe model enabled but Client not billable

Possible causes:

- legal acceptance not completed
- setup session not completed
- webhook not received or not processed
- payment method exists but is not usable for off-session charges
- account locked due to delinquency
- thresholds prohibit auto-approval behaviour

Resolution steps:

1. confirm legal acceptance evidence exists
2. confirm Stripe customer id exists for relationship
3. confirm a default payment method token exists and is attached
4. confirm last webhook outcome
5. confirm account lock state and delinquency status
6. confirm threshold status

### Client claims they already paid / dispute raised

Support must gather:

- booking approval evidence
- charge reference and metadata
- legal authorisation acceptance record
- invoice/receipt history
- supplier references and documents

### Revocation received

Actions:

- mark authorisation revoked and timestamped
- prevent future off-session charges
- determine whether replacement method is required to avoid service disruption
- ensure revocation does not invalidate existing liabilities under the contract
