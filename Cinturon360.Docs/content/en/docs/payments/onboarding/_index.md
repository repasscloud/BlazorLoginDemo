---
title: Onboarding
description: How payment models are enabled and completed for a Client relationship, including Stripe payment method capture.
categories: [platform, payments]
tags: [onboarding, setup]
weight: 10
---

## Onboarding stages

Payment onboarding is a staged process:

1. Contract negotiation: payment model agreed (and terms for auto-approval, thresholds, prepaid/postpaid are confirmed)
2. Configuration: TMC enables the selected model on the Client relationship
3. Legal acceptance: Client accepts authorisation terms (default hosted terms or custom uploaded agreement)
4. Payment setup: if required by model, payment method capture is completed (e.g., Stripe setup session)
5. Billability verification: system marks relationship as billable only after prerequisites are satisfied

Stripe onboarding is the most structured flow and is documented in detail here.
