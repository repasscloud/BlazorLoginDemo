---
title: Disputes and Chargebacks
description: Operational handling for disputes, corrections, and chargebacks for card-based models.
categories: [platform, payments]
tags: [payment-flows]
weight: 50
---

_Last updated: 24 January 2026_

## Dispute types

- Client disputes amount or legitimacy of a charge
- Supplier disputes (supplier claims service delivered / Client claims not delivered)
- Duplicate charges
- Currency conversion issues
- Late cancellation penalties

## Process requirements

When a dispute is raised, the platform MUST be able to produce evidence:

- approval history for the booking
- itinerary and supplier documents
- invoices/receipts issued
- legal authorisation acceptance record
- payment processor charge reference and metadata

## Chargebacks

Chargebacks are card-network processes initiated by the cardholder/issuer.

Operational rule:

- The Client should be directed to use the TMC dispute channel first where contractually required.
- If a chargeback is received, support must assemble evidence from Cinturon360 records.

Dispute notice periods and contractual rules are typically represented in the standing authorisation template and agreement.
