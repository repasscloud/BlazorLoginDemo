---
title: Grace Period
description: How many days beyond due date before an account is locked and requires manual intervention.
categories: [platform, payments]
tags: [financials, payments]
weight: 70
---
## Field

- **Grace Period**: number of days after due date before enforcement applies.

## Purpose

Grace period prevents immediate locking on minor late payments, while still supporting enforcement for delinquent accounts.

## Behaviour

- Due date + grace period defines the enforcement date.
- On enforcement date the account is locked (soft or hard lock, as configured).
- Locking prevents charge initiation and may block approvals.

Unlock requires deliberate action and must be audited.
