---
title: Minimum Monthly Spend
description: How minimum monthly spend is evaluated, reported, and optionally enforced by requesting payment for shortfalls (contract dependent).
categories: [platform, payments]
tags: [financials, payments]
weight: 50
---

_Last updated: 24 January 2026_

## Field

- **Minimum Monthly Spend**: required spend amount per month.
  - if `0`, the minimum spend rule is ignored

## Purpose

Minimum monthly spend can be used to:

- highlight underutilisation of the service
- support CRM and account management workflows
- enforce commercial commitments where contractually agreed

## Evaluation

At end of the month (or configured period), the system computes:

- total eligible spend during the period
- delta between actual spend and required spend

## Outcomes

Depending on configuration and contract:

- report only (create CRM note/task)
- optionally generate a request for payment for the shortfall amount

Any enforcement that creates payable amounts MUST be contractually authorised and documented.
