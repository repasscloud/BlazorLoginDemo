---
title: Legal Placeholders
description: Canonical placeholder definitions and resolution rules for payment authorization documents.
categories: [platform, payments]
tags: [placeholders, legal]
weight: 40
---

## Placeholder resolution

Placeholders are replaced at authorisation request generation time, using the current account configuration for:

- Vendor
- TMC
- Client relationship

Resolution is deterministic. For each placeholder:

- the source is defined (Vendor scope, TMC scope, Client scope)
- the fallback behaviour is defined (if any)
- the output format is defined

## Canonical placeholders (minimum set)

- `{Vendor_Legal_Name}`
- `{Platform_Name}`
- `{TMC_Legal_Name}`
- `{TMC_Trading_Name}`
- `{Client_Legal_Name}`
- `{Client_Trading_Name}`
- `{Client_Billing_Email}`
- `{Payment_Processor}`
- `{Payment_Method}`
- `{Merchant_Of_Record}`
- `{Statement_Descriptor}`
- `{Agreement_Name}`
- `{Agreement_Date}`
- `{Effective_Date}`
- `{Currency_Code}`
- `{Notice_Channel}`
- `{Support_Email}`
- `{Dispute_Notice_Period_Days}`
- `{Revocation_Channel}`

## {Revocation_Channel}

Below is the canonical guidance for `{Revocation_Channel}` used in Client-facing standing authorisations:

# {Revocation_Channel} placeholder

## What {Revocation_Channel} is

`{Revocation_Channel}` is the **client-facing notice channel** for withdrawing (revoking) the standing / ongoing authorization to charge a stored payment method for **approved travel charges**.

In contract and card-network aligned language, it should read as a **method of notice** (i.e., how the Client gives notice to the `{TMC}` that the authorization is withdrawn), and it should be **practical and auditable** (time-stamped, attributable, and easy to evidence in a dispute).

## What it should contain (recommended)

`{Revocation_Channel}` should resolve to **one short sentence** (or a short list) that answers:

- **Where** the Client sends the revocation notice (portal, email, written notice address)
- **Who** receives it (the `{TMC}` billing/accounts contact)
- Optionally **what** the Client must include (e.g., account/booking reference), if you already support that operationally

Avoid wording that implies the Client can revoke by simply “stopping payment” with their bank. Revocation should be framed as **notice to `{TMC}`** (and then `{TMC}` stops initiating future off-session charges).

## Standard, defensible patterns

Choose one of these standard patterns (most common first). Keep it simple.

### Option A (recommended): Portal + email (best auditability)

Use when the Client has a portal/admin area and a known billing email contact.

`{Revocation_Channel}`:

> through the `{Platform_Name}` portal at `{Portal_URL}` (Billing/Payments), or by written notice to `{TMC_Billing_Email}`.

### Option B: Email only (still defensible)

Use when you want one clear channel and you treat email as “written notice”.

`{Revocation_Channel}`:

> by written notice to `{TMC_Billing_Email}`.

### Option C: Written notice (email or postal) + phone for urgent queries (not for revocation itself)

Use when the Client expects a postal address option (enterprise/procurement). Keep phone as “questions”, not as the revocation mechanism, unless you have a recorded, controlled process.

`{Revocation_Channel}`:

> by written notice to `{TMC_Billing_Email}` or `{TMC_Mailing_Address}`. For billing enquiries only, contact `{TMC_Billing_Phone}`.

## Notes that commonly accompany the revocation clause

These are typically handled elsewhere in the clause (not inside `{Revocation_Channel}`), but they are why the channel needs to be auditable:

- Revocation **stops future off-session charges** initiated after the revocation is processed.
- Revocation **does not affect** charges already incurred, bookings already issued, or cancellation/change fees already payable under the `{TMC}`–`{Client}` agreement.
- The Client may be required to **provide a replacement payment method** to avoid disruption to services.

## Recommended placeholders you can support (examples)

- `{Platform_Name}`
- `{Portal_URL}`
- `{TMC_Billing_Email}`
- `{TMC_Mailing_Address}`
- `{TMC_Billing_Phone}`

Keep `{Revocation_Channel}` itself as a **rendered string** assembled from whichever of the above you have for that `{TMC}`.
