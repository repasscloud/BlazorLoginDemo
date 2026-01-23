---
title: Tax and Invoicing
description: How tax rates are applied to invoices and how issuer/recipient roles differ between Vendor→TMC and TMC→Client relationships.
categories: [platform, payments]
tags: [financials, payments]
weight: 40
---

_Last updated: 24 January 2026_

## Tax rate field

- **Tax Rate**: the tax rate applied to invoices issued to the paying party.

Important relationship distinction:

- TMC is a Client of the Vendor (Vendor→TMC billing)
- Client is a Client of the TMC (TMC→Client billing)

Tax rate may differ by relationship and jurisdiction.

## Invoicing responsibilities

Invoice generation MUST clearly identify:

- invoice issuer entity (Vendor or TMC)
- billing recipient entity (TMC or Client)
- tax applied and tax identifiers (where provided)
- service period and line item classification (access fees, travel charges, service fees)

## Payment terms coupling

Invoices always reference the selected payment terms (Net X days) and due dates and are linked to grace period enforcement.
