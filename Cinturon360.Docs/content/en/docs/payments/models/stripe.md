---
title: Stripe
description: Card-based payments using Stripe as the payment processor, including off-session charges and wallet options where enabled.
categories: [platform, payments]
tags: [stripe, card, off-session]
weight: 20
---
## What Stripe means in Cinturon360

Stripe is used as the payment processor for:

- storing payment methods (tokenised credentials)
- charging for approved travel services (often off-session)
- supporting wallets such as Apple Pay and Google Pay where enabled on the TMC Stripe account

Cinturon360 does not store full card PAN/CVV. Payment methods are stored as payment processor tokens and referenced when charging.

## Scope of Stripe enablement

Stripe enablement occurs at TMC scope and is then exposed to Clients:

- If the TMC has Stripe enabled, Stripe becomes an available payment model to that TMC’s Clients.
- Availability does not automatically mean billability. Each Client must complete setup before the account is billable.

## Setup requirements

### TMC scope requirements

- TMC must have a Stripe account configured in Cinturon360
- Stripe capabilities must support off-session charging (where required)
- Wallet methods (Apple Pay / Google Pay) must be enabled on the Stripe account if they are to be offered

### Client scope requirements

- Stripe must be selected as the contract payment model for the Client–TMC relationship
- Client must complete a payment method capture (setup session)
- Client must accept a payment authorization agreement (default or custom)
- A billable default payment method must exist for the Client relationship

## Charge behaviour

Stripe charges are typically initiated as off-session charges when:

- travel services are approved (manual approval) or auto-approved per agreement and policy settings
- a supplier needs ticketing/payment capture
- post-booking changes require additional collection
- service fees are applied
- cancellation/change fees are due

## Failure behaviour

If a Stripe charge fails:

- the transaction is marked as unpaid/past-due (state depends on configuration)
- retry logic may apply (contract and policy dependent)
- account may enter delinquency workflows (see `payments/flows/delinquency-and-locking.md`)

## Evidence and audit expectations

For every Stripe payment method capture and charge, the platform MUST retain evidence:

- client acceptance record (who accepted, timestamp, IP/user agent if captured)
- agreement version or generated agreement copy
- Stripe customer identifier and payment method identifier (token reference)
- charge metadata mapping back to booking/service entities
