---
title: Invoice
description: Postpaid invoicing model where charges are accumulated and invoiced per billing cycle and terms.
categories: [platform, payments]
tags: [invoice, postpaid, terms]
weight: 10
---
## What Invoice means in Cinturon360

Invoice model is used when the Client pays the TMC by invoice based on agreed payment terms.

Typical behaviour:

- charges accumulate as bookings and services occur
- invoices are issued according to billing schedule (contractual)
- payment is expected according to selected payment terms (Net 0/7/14/21/30/60/90)

## Setup requirements

- Invoice payment model selected at Client–TMC relationship
- Billing contacts and invoice delivery channel configured
- Tax rate and invoicing preferences configured (see `payments/financials/tax-and-invoicing.md`)
- Grace period configured to control delinquency handling (see `payments/financials/grace-period.md`)

## Operational notes

- Invoice model still requires “billability” checks: an account can become locked due to delinquency.
- Approval logic still applies: “approved travel charges” remains the definition of what may be invoiced.
