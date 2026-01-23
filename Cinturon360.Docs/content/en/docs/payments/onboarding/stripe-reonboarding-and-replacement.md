---
title: Stripe Re-onboarding and Replacement
description: How payment methods are replaced, revoked, or re-authorised for Stripe-based billing.
categories: [platform, payments]
tags: [stripe, replacement, revocation]
weight: 30
---
## Triggers for re-onboarding

Re-onboarding is required when:

- Client revokes standing authorisation
- payment method expires or is replaced
- off-session charging becomes disallowed by issuer or processor
- charge failures indicate the payment method is no longer valid
- contract changes require new terms or new merchant-of-record

## Replacement rules

- A replacement payment method MUST be captured via a new setup session.
- The relationship MUST be flagged as “not billable” until replacement completes.
- If legal terms changed, the Client MUST accept the updated terms before replacement is used.

## Revocation handling

Revocation is handled via the configured `{Revocation_Channel}`. The revocation must be:

- attributable (who sent it)
- time-stamped
- stored as evidence
- applied prospectively (does not cancel prior liabilities under the contract)

The revocation channel patterns and guidance are documented in the legal placeholders section.
