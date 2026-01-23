---
title: Onboarding Checklist
description: Checklist for TMC and support teams to ensure a Client relationship is billable.
categories: [platform, payments]
tags: [checklist, onboarding]
weight: 20
---
## Stripe checklist (Client relationship)

### Contract and configuration

- Payment model selected = Stripe
- Billing email configured = `{Client_Billing_Email}`
- Merchant of record confirmed = `{Merchant_Of_Record}`
- Statement descriptor configured = `{Statement_Descriptor}` (if required)
- Support email configured = `{Support_Email}`
- Dispute notice period configured = `{Dispute_Notice_Period_Days}` (if contract uses it)
- Revocation channel configured = `{Revocation_Channel}`

### Legal acceptance

- default hosted terms selected, OR custom agreement selected
- rendered copy generated and stored
- acceptance evidence captured

### Stripe setup

- setup session created
- email issued to billing contact
- payment method confirmed by webhook
- default payment method set
- relationship marked billable

### Financial controls

- prepaid/postpaid mode confirmed
- prepaid balance sufficient (if prepaid-only)
- threshold rules configured
- grace period configured (invoice contexts)

If any step fails, the relationship must remain “not billable” and errors must be visible to support.
