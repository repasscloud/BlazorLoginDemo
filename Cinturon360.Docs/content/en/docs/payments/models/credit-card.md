---
title: Credit Card
description: Direct card model (processor-agnostic) for stored credentials and off-session charges.
categories: [platform, payments]
tags: [billing, payment-models]
weight: 60
---

_Last updated: 24 January 2026_

## Summary

Credit Card is a conceptual model representing card-based settlement when the processor is not explicitly Stripe.

In practice, Cinturon360 implements card processing via a processor integration. The requirements here apply to any card processor:

## Required behaviour

- card details MUST be stored tokenised by the processor
- off-session charges MUST only occur after Client authorisation acceptance
- revocation channel MUST be defined and auditable
- evidence MUST be captured for acceptance, charges, refunds, and disputes

When implemented, this model SHOULD either be:
- mapped to Stripe (preferred), or
- mapped to another processor with equivalent stored-credential support.
