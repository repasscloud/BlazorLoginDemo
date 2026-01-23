---
title: PayPal
description: Payments using PayPal as the settlement mechanism, subject to TMC enablement and contract.
categories: [platform, payments]
tags: [paypal]
weight: 30
---
## Current scope

PayPal is documented as a supported model. Implementation specifics depend on integration decisions.

This page defines what MUST be documented and enforced once PayPal is enabled in production:

## Required configuration

### TMC scope

- PayPal account identifiers and settlement configuration
- Enabled currencies
- Payment capture mode (immediate vs authorise/capture)

### Client scope

- PayPal model selected by contract
- Client payment authorisation flow completed (if required for recurring/off-session equivalents)

## Billability rules

PayPal MUST define:

- whether “stored credential” equivalents exist
- whether payments are initiated off-session, on-session, or via invoice-like requests
- how failures, retries, and delinquency work

Until PayPal is fully implemented, this model should be treated as **not available** in production configurations.
