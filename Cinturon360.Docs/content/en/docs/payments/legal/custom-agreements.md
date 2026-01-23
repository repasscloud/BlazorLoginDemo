---
title: Custom Uploaded Agreements
description: How Vendors and TMCs upload their own legal agreements and use placeholders to generate Client-facing authorizations.
categories: [platform, payments]
tags: [legal, custom, placeholders]
weight: 30
---

_Last updated: 24 January 2026_

## Summary

Vendors and TMCs may elect to use their own Client-facing agreements instead of the platform default.

This is common when:

- the TMC already has a Master Services Agreement (MSA) and wants consistent document sets
- the Vendor operates as a white-label provider and wants each reseller/TMC to use their own terms
- the contract specifies alternative dispute, revocation, or invoicing rules

## Upload and selection model

### Storage

- Agreements are uploaded to the account context (Vendor or TMC scope).
- Agreements can be selected per Client–TMC relationship.
- Agreements MUST be versioned (new upload = new version).

### Selection

For each Client relationship the system must store:

- which agreement is selected (default hosted vs custom)
- the effective date/version
- the placeholders expected by that agreement

## Rendering and issuance

When an authorisation request is created, the platform MUST:

1. select the agreement (default hosted or custom)
2. render placeholders using the current account configuration
3. generate an immutable copy (PDF/HTML/markdown snapshot per implementation)
4. issue that copy to the Client (portal + email link)
5. store the exact rendered copy for audit

## Placeholder requirements

Custom agreements MUST use dedicated placeholders so Cinturon360 can generate correct Client-facing content.

At minimum, agreements used for off-session charging should support:

- `{Client_Legal_Name}`
- `{TMC_Legal_Name}`
- `{TMC_Trading_Name}`
- `{Payment_Processor}` / `{Payment_Method}`
- `{Merchant_Of_Record}`
- `{Agreement_Name}` / `{Agreement_Date}` (if cross-referencing an MSA)
- `{Effective_Date}`
- `{Notice_Channel}`
- `{Revocation_Channel}`

Additional placeholders may exist for tax, fees, support contacts, and jurisdictional notices.

## Enforcement rule

If a selected custom agreement contains placeholders that cannot be resolved, the platform MUST NOT issue the authorisation request until resolved.

This prevents issuing legally ambiguous documents.
