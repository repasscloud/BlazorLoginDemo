---
title: Google Pay
description: Wallet payment method available via processor enablement (typically Stripe).
categories: [platform, payments]
tags: [billing, payment-models]
weight: 80
---
## Summary

Google Pay is offered as a payment method when the underlying processor supports it and the TMC enables it.

### Key rule

Google Pay availability is derived from the TMC’s processor capabilities. If Google Pay is disabled on the TMC’s Stripe account (or equivalent), it MUST NOT be presented as available to Clients.

## Operational notes

- Google Pay payments may still result in stored-credential tokens depending on processor behaviour.
- Off-session charges require explicit authorisation and stored credential support.
