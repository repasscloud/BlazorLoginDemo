---
title: Billing & Fee Architecture
description: How platform fees, payment processing costs, card classification, and margins are calculated across Vendor, TMC, and Client layers.
type: docs
weight: 10
---

This section documents the **authoritative billing and fee model** used by the platform.

It explains, in detail:

- Why pass-through costs must be preserved
- How payment processor fees (Stripe today) affect pricing
- How platform fees are calculated to cover those costs
- How card brand, country, and funding influence fee assumptions
- How the same logic applies across **Vendor → TMC → Client**
- How developers should implement and extend this safely

These documents are written for:
- **Vendors** (commercial and finance)
- **TMCs** (commercial, finance, operations)
- **Internal developers** (billing, payments, invoicing)

This is not marketing documentation. It describes **how the system actually works and why**.
